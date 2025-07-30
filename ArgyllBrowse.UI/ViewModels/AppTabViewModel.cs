using ArgyllBrowse.Data.Services;
using ArgyllBrowse.UI.ViewModels.Contracts;
using ArgyllBrowse.UI.ViewModels.Helpers;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Policy;
using System.Threading.Tasks;

namespace ArgyllBrowse.UI.ViewModels;
public partial class AppTabViewModel : ReactiveObject, IDisposable
{
    private IReactiveWebView ReactiveWebView { get; set; } = null!;
    public IObservable<string> DocumentTitleChanges => ReactiveWebView.DocumentTitleChanges.AsObservable();
    public IObservable<string> FaviconUrl => ReactiveWebView.FaviconUrlChanges.AsObservable();
    public IObservable<Unit> NavigationStarting => ReactiveWebView.NavigationStarting.AsObservable();
    public IObservable<Unit> NavigationCompleted => ReactiveWebView.NavigationCompleted.AsObservable();

    private int TabId { get; set; }

    [Reactive]
    public partial bool CanStop { get; set; }

    [Reactive]
    public partial bool CanRefresh { get; set; }

    [Reactive]
    public partial int? Index { get; set; }

    [Reactive]
    public partial bool IsTabSelected { get; set; }

    [Reactive]
    private partial Uri Url { get; set; } = Constants.AboutBlankUri;

    [Reactive]
    public partial string SearchBarText { get; set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> NavigateToSeachBarInput { get; }
    public ReactiveCommand<Unit, Unit> Refresh { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoBack { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoForward { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> Stop { get; private set; } = null!;

    private BrowserDataService DataService { get; }

    public AppTabViewModel(BrowserDataService browserDataService)
    {
        ArgumentNullException.ThrowIfNull(browserDataService);
        DataService = browserDataService;

        NavigateToSeachBarInput = ReactiveCommand.Create(NavigateToSeachBarInputImpl);

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
    }

    private async Task SaveTabStateAsync()
    {
        await DataService.SaveTabStateAsync(TabId, Index!.Value, Url, IsTabSelected);
    }

    private void NavigateToSeachBarInputImpl()
    {
        Uri url = Uri.TryCreate(SearchBarText, UriKind.Absolute, out Uri? result)
            ? result.EnforceHttps()
            : new Uri($"https://duckduckgo.com/?q={SearchBarText}");

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
