using ArgyllBrowse.Data.Services;
using ArgyllBrowse.UI.Helpers;
using ArgyllBrowse.UI.ViewModels.Contracts;
using ArgyllBrowse.UI.ViewModels.Helpers;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ArgyllBrowse.UI.ViewModels;
public partial class AppTabViewModel : ReactiveObject, IDisposable
{
    private const string httpsPrefix = "https://";
    private const string httpPrefix = "http://";

    private IReactiveWebView ReactiveWebView { get; set; } = null!;
    public IObservable<string> DocumentTitle => ReactiveWebView.DocumentTitle.AsObservable();
    public IObservable<string> FaviconUrl => ReactiveWebView.FaviconUrl.AsObservable();
    public IObservable<Unit> NavigationStarting => ReactiveWebView.NavigationStarting.AsObservable();
    public IObservable<Unit> NavigationCompleted => ReactiveWebView.NavigationCompleted.AsObservable();
    public IObservable<bool> Isloading => ReactiveWebView.IsLoading.AsObservable();

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

    public ReactiveCommand<Unit, Unit> NavigateToSeachBarInput { get; }
    public ReactiveCommand<Unit, Unit> Refresh { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoBack { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoForward { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> Stop { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> OpenConfig { get; private set; } = null!;

    private BrowserDataService DataService { get; } = ServiceLocator.GetRequiredService<BrowserDataService>();

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

        NavigateToSeachBarInput = ReactiveCommand.Create(NavigateToSeachBarInputImpl);
        OpenConfig = ReactiveCommand.Create(() => { });

        this.WhenAnyValue(x => x.Index)
            .WhereNotNull()
            .Subscribe(async index => await DataService.SetTabIndexAsync(TabId, index!.Value));

        this.WhenAnyValue(x => x.Url)
            .WhereNotNull()
            .Subscribe(async url => await DataService.SetTabUrlAsync(TabId, url));

        this.WhenAnyValue(x => x.IsTabSelected)
            .Subscribe(async isSelected => await DataService.SetIsTabSelectedAsync(TabId, isSelected));

        this.WhenAnyValue(x => x.Url)
            .WhereNotNull()
            .Subscribe(url => UpdateSearchBar());

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

        FaviconUrl
            .Subscribe(async faviconUrl => await DataService.SaveTabFaviconUrlAsync(TabId, faviconUrl));

        DocumentTitle
            .Subscribe(async documentTitle => await DataService.SaveTabDocumentTitleAsync(TabId, documentTitle));
    }

    private void NavigateToSeachBarInputImpl()
    {
        string text = SearchBarText.Contains('.', StringComparison.OrdinalIgnoreCase)
            && SearchBarText[0] != '.'
            && SearchBarText[^1] != '.'
                && (SearchBarText.StartsWith(httpsPrefix, StringComparison.OrdinalIgnoreCase)
                    || SearchBarText.StartsWith(httpPrefix, StringComparison.OrdinalIgnoreCase))
        ? SearchBarText
        : httpPrefix + SearchBarText;

        Uri url;

        if (Uri.TryCreate(text, UriKind.Absolute, out Uri? result))
        {
            url = result;
        }
        else
        {
            url = new Uri($"https://duckduckgo.com/?q={SearchBarText}");
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
