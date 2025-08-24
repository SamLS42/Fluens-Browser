using ArgyllBrowse.AppCore.Contracts;
using ArgyllBrowse.AppCore.Helpers;
using ArgyllBrowse.AppCore.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;
using System.Reactive.Linq;

namespace ArgyllBrowse.AppCore.ViewModels;
public partial class AppTabViewModel : ReactiveObject, IDisposable
{
    private const string httpsPrefix = "https://";
    private const string httpPrefix = "http://";

    private IReactiveWebView ReactiveWebView { get; set; } = null!;
    public IObservable<string> DocumentTitle => ReactiveWebView.DocumentTitle.AsObservable();
    public IObservable<string> FaviconUrl => ReactiveWebView.FaviconUrl.AsObservable();
    public IObservable<bool> IsLoading => ReactiveWebView.IsLoading.AsObservable();

    public int TabId { get; set; }

    [Reactive]
    public partial bool CanStop { get; set; }

    [Reactive]
    public partial bool CanRefresh { get; set; }

    [Reactive]
    public partial int? Index { get; set; }

    [Reactive]
    public partial bool IsTabSelected { get; set; }

    [Reactive]
    public partial Uri Url { get; set; } = null!;

    [Reactive]
    public partial string SearchBarText { get; set; } = string.Empty;

    [Reactive]
    public partial bool SettingsDialogIsOpen { get; set; }

    public ReactiveCommand<Unit, Unit> NavigateToSearchBarInput { get; }
    public ReactiveCommand<Unit, Unit> Refresh { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoBack { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoForward { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> Stop { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ToggleSettingsDialogIsOpen { get; private set; }

    private TabPersistencyService DataService { get; } = ServiceLocator.GetRequiredService<TabPersistencyService>();
    private HistoryService HistoryService { get; } = ServiceLocator.GetRequiredService<HistoryService>();

    public AppTabViewModel(int tabId = 0, int? index = null)
    {
        Index = index;

        if (tabId == 0)
        {
            TabId = DataService.CreateTab(Constants.AboutBlankUri);
        }
        else
        {
            TabId = tabId;
        }

        ToggleSettingsDialogIsOpen = ReactiveCommand.Create(() => { SettingsDialogIsOpen = !SettingsDialogIsOpen; });

        NavigateToSearchBarInput = ReactiveCommand.Create(NavigateToSearchBarInputImpl);

        this.WhenAnyValue(x => x.Index)
            .WhereNotNull()
            .Subscribe(async index => await DataService.SetTabIndexAsync(TabId, index!.Value));

        this.WhenAnyValue(x => x.Url)
            .WhereNotNull()
            .Subscribe(async url =>
            {
                await DataService.SetTabUrlAsync(TabId, url);
                UpdateSearchBar();
            });

        this.WhenAnyValue(x => x.IsTabSelected)
            .Subscribe(async isSelected => await DataService.SetIsTabSelectedAsync(TabId, isSelected));
    }

    private async Task UpdateHistoryAsync(CancellationToken cancellationToken = default)
    {
        await HistoryService.AddEntryAsync(ReactiveWebView.Url.Value, ReactiveWebView.FaviconUrl.Value, ReactiveWebView.DocumentTitle.Value, cancellationToken);
    }

    public void SetReactiveWebView(IReactiveWebView reactiveWebView)
    {
        ReactiveWebView = reactiveWebView;

        GoBack = ReactiveCommand.Create(ReactiveWebView.GoBack);
        GoForward = ReactiveCommand.Create(ReactiveWebView.GoForward);
        Refresh = ReactiveCommand.Create(ReactiveWebView.Refresh);
        Stop = ReactiveCommand.Create(ReactiveWebView.StopNavigation);

        ReactiveWebView.IsLoading.Subscribe(SetStopRefreshVisibility);
        ReactiveWebView.Url.Subscribe(url => Url = url);
        FaviconUrl.Subscribe(async faviconUrl => await DataService.SaveTabFaviconUrlAsync(TabId, faviconUrl));
        DocumentTitle.Subscribe(async documentTitle => await DataService.SaveTabDocumentTitleAsync(TabId, documentTitle));

        ReactiveWebView.NavigationCompleted
            .Merge(ReactiveWebView.FaviconUrl.Where(faviconUrl => !string.IsNullOrWhiteSpace(faviconUrl)).Select(_ => Unit.Default))
            .Throttle(TimeSpan.FromSeconds(1))
            .Subscribe(async _ => await UpdateHistoryAsync());
    }

    private void NavigateToSearchBarInputImpl()
    {
        // Normalize input
        var search = (SearchBarText ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(search))
            return;

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
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var url))
        {
            var query = Uri.EscapeDataString(search);
            url = new Uri($"https://duckduckgo.com/?q={query}");
        }

        ReactiveWebView?.NavigateToUrl(url);
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
        ReactiveWebView.Dispose();

        Observable.FromAsync(_ => DataService.DeleteTabAsync(TabId)).Subscribe();
    }
}
