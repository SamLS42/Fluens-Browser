using ArgyllBrowse.Data.Services;
using ArgyllBrowse.UI.ViewModels.Contracts;
using ArgyllBrowse.UI.ViewModels.Helpers;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ArgyllBrowse.UI.ViewModels;
public partial class AppTabViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable Disposables = [];

    private IReactiveWebView ReactiveWebView { get; set; } = null!;
    public IObservable<string> DocumentTitleChanges => ReactiveWebView.DocumentTitleChanges.AsObservable();
    public IObservable<string> FaviconUrl => ReactiveWebView.FaviconUrlChanges.AsObservable();
    public IObservable<Unit> NavigationStarting => ReactiveWebView.NavigationStarting.AsObservable();
    public IObservable<Unit> NavigationCompleted => ReactiveWebView.NavigationCompleted.AsObservable();

    private readonly Subject<Unit> disposed = new();
    public IObservable<Unit> Disposed => disposed.AsObservable();

    private int TabId { get; set; }

    public bool CanStop
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool CanRefresh
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int? Index
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsTabSelected
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
            .WhereNotNull()
            .Select(_ => Unit.Default)
            .Merge(this.WhenAnyValue(x => x.Url)
                .SkipWhile(_ => Index is null)
                .Select(_ => Unit.Default))
            .Merge(this.WhenAnyValue(x => x.IsTabSelected)
                .SkipWhile(_ => Index is null)
                .Select(_ => Unit.Default))
            .Throttle(TimeSpan.FromSeconds(1))
            .Subscribe(async _ => await SaveTabStateAsync());

        this.WhenAnyValue(x => x.Url)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => UpdateSearchBar());
    }

    public void SetReactiveWebView(IReactiveWebView reactiveWebView)
    {
        ReactiveWebView = reactiveWebView;

        GoBack = ReactiveCommand.Create(ReactiveWebView.GoBack);
        GoForward = ReactiveCommand.Create(ReactiveWebView.GoForward);
        Refresh = ReactiveCommand.Create(ReactiveWebView.Refresh);
        Stop = ReactiveCommand.Create(ReactiveWebView.StopNavigation);


        ReactiveWebView.IsLoading.Subscribe(SetStopRefreshVisibility)
            .DisposeWith(Disposables);
    }

    private async Task SaveTabStateAsync()
    {
        await DataService.SaveTabStateAsync(TabId, Index!.Value, Url, IsTabSelected);
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
        Disposables.Dispose();
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
}
