using ArgyllBrowse.Data.Services;
using ArgyllBrowse.UI.ViewModels.Contracts;
using ArgyllBrowse.UI.ViewModels.Helpers;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ArgyllBrowse.UI.ViewModels;
public partial class AppTabViewModel : ReactiveObject, IDisposable
{
    private IReactiveWebView ReactiveWebView { get; set; } = null!;

    public IObservable<bool> IsLoading => ReactiveWebView.IsLoading.AsObservable();
    public IObservable<string> DocumentTitleChanges => ReactiveWebView.DocumentTitleChanges.AsObservable();
    public IObservable<string> FaviconUrl => ReactiveWebView.FaviconUrlChanges.AsObservable();
    public IObservable<Unit> NavigationStarting => ReactiveWebView.NavigationStarting.AsObservable();
    public IObservable<Unit> NavigationCompleted => ReactiveWebView.NavigationCompleted.AsObservable();


    private readonly Subject<Unit> disposed = new();
    public IObservable<Unit> Disposed => disposed.AsObservable();

    private int TabId { get; set; }
    public int Index
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Uri Url
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = new Uri("about:blank");

    private const string httpSufix = "http://";
    private const string httpsSufix = "https://";

    public string SearchBarText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    public ReactiveCommand<Unit, Unit> NavigateToUrl { get; }
    public ReactiveCommand<Unit, Unit> Refresh { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoBack { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoForward { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> Stop { get; private set; } = null!;

    private BrowserDataService DataService { get; }

    public AppTabViewModel(BrowserDataService browserDataService)
    {
        ArgumentNullException.ThrowIfNull(browserDataService);
        DataService = browserDataService;

        NavigateToUrl = ReactiveCommand.Create(NavigateToUrlImpl);

        TabId = browserDataService.CreateTab(Url);

        this.WhenAnyValue(x => x.Index)
            .Where(index => index != 0)
            .DistinctUntilChanged()
            .Select(_ => Unit.Default)
            .Merge(this.WhenAnyValue(x => x.Url)
                .DistinctUntilChanged()
                .Select(_ => Unit.Default))
            .Throttle(TimeSpan.FromSeconds(1))
            .Subscribe(async _ => await SaveTabStateAsync());

        this.WhenAnyValue(x => x.Url)
            .Throttle(TimeSpan.FromMilliseconds(500)) //Sometime, the URL is set the about:blank during navigation, is a WebView2 behavior (I think)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => UpdateSearchBar());
    }

    public void SetEeactiveWebView(IReactiveWebView reactiveWebView)
    {
        ReactiveWebView = reactiveWebView;

        GoBack = ReactiveCommand.Create(ReactiveWebView.GoBack);
        GoForward = ReactiveCommand.Create(ReactiveWebView.GoForward);
        Refresh = ReactiveCommand.Create(ReactiveWebView.Refresh);
        Stop = ReactiveCommand.Create(ReactiveWebView.StopNavigation);
    }

    private async Task SaveTabStateAsync()
    {
        await DataService.SaveTabStateAsync(TabId, Index, Url);
    }

    private void NavigateToUrlImpl()
    {
        SetUrlPrefix();

        if (Uri.TryCreate(SearchBarText, UriKind.Absolute, out Uri? result))
        {
            Url = result;
        }
        else
        {
            //TODO: Search using selected search engine
        }
    }

    private void SetUrlPrefix()
    {
        if (SearchBarText.StartsWith(httpsSufix, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (SearchBarText.StartsWith(httpSufix, StringComparison.OrdinalIgnoreCase))
        {
            SearchBarText = SearchBarText.Replace(httpSufix, httpsSufix, StringComparison.OrdinalIgnoreCase);
            return;
        }

        SearchBarText = httpsSufix + SearchBarText;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool dispose)
    {
        disposed.OnNext(Unit.Default);
        disposed.OnCompleted();
        disposed.Dispose();
        ReactiveWebView.Dispose();
        Observable.FromAsync(_ => DataService.DeleteTabAsync(TabId)).Subscribe();
    }

    private void UpdateSearchBar()
    {
        string text = Url.ToString();

        SearchBarText = text.Equals(Constants.AboutBlankUri, StringComparison.Ordinal)
            ? string.Empty
            : text;
    }
}
