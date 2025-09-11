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
    public partial IObservableWebView? ReactiveWebView { get; set; }

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

    public ReactiveCommand<Unit, Unit> NavigateToInputComman { get; }
    public ReactiveCommand<Unit, Unit> Refresh { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoBack { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoForward { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> Stop { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ToggleSettingsDialogCommand { get; private set; }

    private TabPersistencyService TabPersistencyService { get; } = ServiceLocator.GetRequiredService<TabPersistencyService>();
    private HistoryService HistoryService { get; } = ServiceLocator.GetRequiredService<HistoryService>();
    private ITabPageManager TabPageManager { get; } = ServiceLocator.GetRequiredService<ITabPageManager>();

    public AppTabViewModel()
    {
        ToggleSettingsDialogCommand = ReactiveCommand.Create(() => { SettingsDialogIsOpen = !SettingsDialogIsOpen; });

        NavigateToInputComman = ReactiveCommand.Create(NavigateToInput);

        this.WhenAnyValue(x => x.Index, x => x.Id, (index, id) => new { index, id })
            .Where(i => i.index != null && i.id > 0)
            .Subscribe(async i => await TabPersistencyService.SetTabIndexAsync(i.id, i.index!.Value));

        this.WhenAnyValue(x => x.Url, x => x.Id, (url, id) => new { url, id })
            .Where(i => i.url != null && i.id > 0)
            .Subscribe(async i => await TabPersistencyService.SetTabUrlAsync(i.id, i.url));

        this.WhenAnyValue(x => x.ParentWindowId, x => x.Id, (parentWindowId, id) => new { parentWindowId, id })
            .Where(i => i.id > 0 && i.parentWindowId > 0)
            .Subscribe(async i => await TabPersistencyService.SetWindowAsync(i.id, i.parentWindowId));

        this.WhenAnyValue(x => x.FaviconUrl, x => x.Id, (faviconUrl, id) => new { faviconUrl, id })
            .Where(i => i.id > 0)
            .Subscribe(async i => await TabPersistencyService.SaveTabFaviconUrlAsync(i.id, i.faviconUrl));

        this.WhenAnyValue(x => x.DocumentTitle, x => x.Id, (documentTitle, id) => new { documentTitle, id })
            .Where(i => i.id > 0)
            .Subscribe(async i => await TabPersistencyService.SaveTabDocumentTitleAsync(i.id, i.documentTitle));

        this.WhenAnyValue(x => x.IsSelected, x => x.Id, (isSelected, id) => new { isSelected, id })
            .Where(i => i.id > 0)
            .Subscribe(async i => await TabPersistencyService.SetIsTabSelectedAsync(i.id, i.isSelected));

        this.WhenAnyValue(x => x.Url, x => x.FaviconUrl, x => x.DocumentTitle, (url, faviconUrl, documentTitle) => new { url, faviconUrl, documentTitle })
            .Where(obj => obj.url != null)
            .Throttle(TimeSpan.FromSeconds(2))
            .Subscribe(async i => await HistoryService.AddEntryAsync(i.url, i.faviconUrl, i.documentTitle));

        this.WhenAnyValue(x => x.Url)
            .WhereNotNull()
            .Subscribe(_ => UpdateSearchBar());

        this.WhenAnyValue(x => x.ReactiveWebView)
            .WhereNotNull()
            .Subscribe(_ =>
            {
                GoBack = ReactiveCommand.Create(ReactiveWebView!.GoBack);
                GoForward = ReactiveCommand.Create(ReactiveWebView.GoForward);
                Refresh = ReactiveCommand.Create(ReactiveWebView.Refresh);
                Stop = ReactiveCommand.Create(ReactiveWebView.StopNavigation);

                ReactiveWebView.IsNavigating.Subscribe(SetStopRefreshVisibility);
                ReactiveWebView.Url.Subscribe(url => Url = url);
                ReactiveWebView.DocumentTitle.Subscribe(documentTitle => DocumentTitle = documentTitle);
                ReactiveWebView.FaviconUrl.Subscribe(faviconUrl => FaviconUrl = faviconUrl);
                ReactiveWebView.OpenNewTab.Subscribe(async uri =>
                {
                    IViewFor<AppPageViewModel> page = TabPageManager.GetParentTabPage(this);
                    AppTabViewModel vm = await page.ViewModel!.CreateTabAsync(uri);
                    page.ViewModel.CreateTabViewItem(vm);
                    page.ViewModel.SelectItem(vm);
                });
                ReactiveWebView.KeyboardShortcuts.Subscribe(KeyboardShortcutsSource.OnNext);
            });

        this.WhenAnyValue(x => x.IsSelected, x => x.ReactiveWebView, (isSelected, web) => isSelected && web != null)
            .DistinctUntilChanged()
            .Where(ready => ready)
            .Subscribe(_ => Activate());
    }

    public void ShortcutMessageInvoked(ShortcutMessage shortcutMessage)
    {
        KeyboardShortcutsSource.OnNext(shortcutMessage);
    }

    private void NavigateToInput()
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

        ReactiveWebView?.NavigateToUrl(url);
    }

    public void Activate()
    {
        if (Url == Constants.AboutBlankUri && ReactiveWebView!.Source is null)
        {
            return;
        }

        if (Url != ReactiveWebView!.Source)
        {
            ReactiveWebView.NavigateToUrl(Url);
        }
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
            ReactiveWebView?.Dispose();
        }
    }
}
