using Fluens.AppCore.Contracts;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Fluens.AppCore.ViewModels;

public partial class AppTabViewModel : ReactiveObject, IDisposable
{
    private const string httpsPrefix = "https://";
    private const string httpPrefix = "http://";

    public IObservable<ShortcutMessage> KeyboardShortcuts => KeyboardShortcutsSource.AsObservable();
    private Subject<ShortcutMessage> KeyboardShortcutsSource { get; } = new();

    [Reactive]
    public partial int Id { get; set; }

    [Reactive]
    public partial IObservableWebView? ObservableWebView { get; set; }

    [Reactive]
    public partial string DocumentTitle { get; set; } = string.Empty;

    [Reactive]
    public partial string FaviconUrl { get; set; } = string.Empty;

    [Reactive]
    public partial bool IsLoading { get; set; }

    [Reactive]
    public partial bool CanStop { get; set; }

    [Reactive]
    public partial bool CanRefresh { get; set; }

    [Reactive]
    public partial int? Index { get; set; }

    [Reactive]
    public partial bool IsSelected { get; set; }

    [Reactive]
    public partial Uri Url { get; set; } = null!;

    [Reactive]
    public partial string SearchBarText { get; set; } = string.Empty;

    [Reactive]
    public partial bool SettingsDialogIsOpen { get; set; }

    [Reactive]
    public partial int ParentWindowId { get; set; }

    public ReactiveCommand<Unit, Unit> Refresh { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoBack { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoForward { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> Stop { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ToggleSettingsDialogCommand { get; private set; }

    private TabPersistencyService TabPersistencyService { get; } = ServiceLocator.GetRequiredService<TabPersistencyService>();
    private VisitsService VisitsService { get; } = ServiceLocator.GetRequiredService<VisitsService>();
    private PlacesService PlacesService { get; } = ServiceLocator.GetRequiredService<PlacesService>();
    private ITabPageManager TabPageManager { get; } = ServiceLocator.GetRequiredService<ITabPageManager>();

    public AppTabViewModel()
    {
        ToggleSettingsDialogCommand = ReactiveCommand.Create(() => { SettingsDialogIsOpen = !SettingsDialogIsOpen; });

        this.WhenAnyValue(x => x.Index, x => x.Id, (index, id) => new { index, id })
            .Where(i => i.index != null && i.id > 0)
            .Subscribe(async i => await TabPersistencyService.UpdateTabInfoAsync(i.id, index: i.index));

        this.WhenAnyValue(x => x.ParentWindowId, x => x.Id, (parentWindowId, id) => new { parentWindowId, id })
            .Where(i => i.id > 0 && i.parentWindowId > 0)
            .Subscribe(async i => await TabPersistencyService.UpdateTabInfoAsync(i.id, windowId: i.parentWindowId));

        this.WhenAnyValue(x => x.IsSelected, x => x.Id, (IsSelected, id) => new { IsSelected, id })
            .Where(i => i.id > 0)
            .Subscribe(async i => await TabPersistencyService.UpdateTabInfoAsync(i.id, isSelected: i.IsSelected));

        // Updates the place in this tab and the place's favicon and title
        this.WhenAnyValue(x => x.Url, x => x.Id, (url, id) => new { url, id })
            .Where(i => i.url != null && i.url != Constants.AboutBlankUri && i.id > 0)
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Select(i =>
                Observable.FromAsync(async ct =>
                {
                    int placeId = await PlacesService.GetorCreatePlaceAsync(i.url, ct);
                    await VisitsService.AddEntryAsync(placeId, ct);
                    await TabPersistencyService.UpdateTabInfoAsync(i.id, placeId: placeId, cancellationToken: ct);
                    return placeId;
                })
                .SelectMany(placeId => Observable.Merge(
                        this.WhenAnyValue(x => x.FaviconUrl)
                            .DistinctUntilChanged()
                            .Where(v => !string.IsNullOrWhiteSpace(v) && v != Constants.LoadingFaviconUri)
                            .SelectMany(v => Observable.FromAsync(() => PlacesService.UpdatePlaceAsync(placeId, faviconUrl: v))),
                        this.WhenAnyValue(x => x.DocumentTitle)
                            .DistinctUntilChanged()
                            .Where(v => !string.IsNullOrWhiteSpace(v))
                            .SelectMany(v => Observable.FromAsync(() => PlacesService.UpdatePlaceAsync(placeId, title: v))))
                ))
            .Switch()
            .Subscribe();

        this.WhenAnyValue(x => x.Url)
            .WhereNotNull()
            .Subscribe(_ => UpdateSearchBar());

        this.WhenAnyValue(x => x.ObservableWebView)
            .WhereNotNull()
            .Subscribe(webView =>
            {
                GoBack = ReactiveCommand.Create(webView!.GoBack);
                GoForward = ReactiveCommand.Create(webView.GoForward);
                Refresh = ReactiveCommand.Create(webView.Refresh);
                Stop = ReactiveCommand.Create(webView.StopNavigation);

                webView.IsNavigating.Subscribe(SetStopRefreshVisibility);
                webView.Url.Subscribe(url => Url = url);
                webView.DocumentTitle.Subscribe(documentTitle => DocumentTitle = documentTitle);
                webView.FaviconUrl.Subscribe(faviconUrl => FaviconUrl = faviconUrl);
                webView.OpenNewTab.Subscribe(async uri =>
                {
                    IViewFor<AppPageViewModel> page = TabPageManager.GetParentTabPage(this);
                    AppTabViewModel vm = await page.ViewModel!.CreateTabAsync(uri);
                    page.ViewModel.CreateTabViewItem(vm);
                    vm.Activate();
                });
                webView.KeyboardShortcuts.Subscribe(KeyboardShortcutsSource.OnNext);
            });

        this.WhenAnyValue(x => x.IsSelected, x => x.ObservableWebView, x => x.Url, (isSelected, web, url) => isSelected && web != null && url != Constants.AboutBlankUri)
            .DistinctUntilChanged()
            .Where(ready => ready)
            .Subscribe(_ => Activate());
    }

    public void ShortcutMessageInvoked(ShortcutMessage shortcutMessage)
    {
        KeyboardShortcutsSource.OnNext(shortcutMessage);
    }

    [ReactiveCommand]
    private async Task NavigateToInput()
    {
        // Normalize input
        string search = (SearchBarText ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(search))
        {
            return;
        }

        bool containsDot = search.Contains('.', StringComparison.Ordinal);
        bool startsOrEndsWithDot = search.StartsWith('.') || search.EndsWith('.');
        bool hasScheme = search.StartsWith(httpsPrefix, StringComparison.OrdinalIgnoreCase)
                      || search.StartsWith(httpPrefix, StringComparison.OrdinalIgnoreCase);

        // Follow original logic: only use the raw input as a URL if it contains a dot, does not start/end with a dot,
        // and already begins with http(s). Otherwise prepend https://

        string candidate;

        if (containsDot && !startsOrEndsWithDot && !hasScheme)
        {
            candidate = httpsPrefix + search;
        }
        else
        {
            candidate = search;
        }

        // Try to make an absolute Uri; if that fails, fall back to a search query (DuckDuckGo)
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out Uri? url))
        {
            string query = Uri.EscapeDataString(search);
            url = new Uri($"https://duckduckgo.com/?q={query}");
        }

        await (ObservableWebView?.NavigateToUrlAsync(url) ?? Task.CompletedTask);
    }

    public void Activate()
    {
        this.WhenAnyValue(x => x.ObservableWebView)
            .WhereNotNull()
            .Take(1)
            .Subscribe(async wv =>
            {
                await wv.ActivateAsync();

                if (Url == Constants.AboutBlankUri && ObservableWebView!.Source is null)
                {
                    return;
                }

                if (Url != ObservableWebView!.Source)
                {
                    await ObservableWebView!.NavigateToUrlAsync(Url);
                }
            });
    }

    private void UpdateSearchBar()
    {
        string text = Url.ToString();

        SearchBarText = text.Equals(Constants.AboutBlankUri.ToString(), StringComparison.Ordinal)
            ? string.Empty
            : text;
    }

    private void SetStopRefreshVisibility(bool showStopBtn)
    {
        if (showStopBtn)
        {
            CanStop = true;
            CanRefresh = false;
        }
        else
        {
            CanStop = false;
            CanRefresh = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool dispose)
    {
        if (dispose)
        {
            ObservableWebView?.Dispose();
        }
    }
}
