using ArgyllBrowse.Data.Services;
using ArgyllBrowse.UI.ViewModels.Contracts;
using ArgyllBrowse.UI.ViewModels.Helpers;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Diagnostics.CodeAnalysis;
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

    private int? TabId { get; set; }

    [Reactive]
    public partial bool CanStop { get; set; }

    [Reactive]
    public partial bool CanRefresh { get; set; }

    [Reactive]
    public partial int? Index { get; set; }

    [Reactive]
    public partial bool IsTabSelected { get; set; }

    [Reactive]
    private partial Uri Url { get; set; } = null!;

    [Reactive]
    public partial string SearchBarText { get; set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> NavigateToSeachBarInput { get; }
    public ReactiveCommand<Unit, Unit> Refresh { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoBack { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> GoForward { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> Stop { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> OpenConfig { get; private set; } = null!;

    private BrowserDataService DataService { get; }

    public AppTabViewModel(BrowserDataService dataService)
    {
        ArgumentNullException.ThrowIfNull(dataService);
        DataService = dataService;

        NavigateToSeachBarInput = ReactiveCommand.Create(NavigateToSeachBarInputImpl);
        OpenConfig = ReactiveCommand.Create(() => { });

        this.WhenAnyValue(x => x.Index)
            .WhereNotNull()
            .Select(_ => Unit.Default)
            .Merge(this.WhenAnyValue(x => x.Url)
                .SkipWhile(_ => Index is null)
                .Select(_ => Unit.Default))
            .Merge(this.WhenAnyValue(x => x.IsTabSelected)
                .SkipWhile(_ => Index is null)
                .Select(_ => Unit.Default))
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Subscribe(async _ => await SaveTabStateAsync());

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
    }

    private async Task SaveTabStateAsync()
    {
        TabId ??= DataService.CreateTab(Url);
        await DataService.SaveTabStateAsync(TabId.Value, Index!.Value, Url, IsTabSelected, ReactiveWebView.FaviconUrlChanges.Value, ReactiveWebView.DocumentTitleChanges.Value);
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

        if (TabId != null)
        {
            Observable.FromAsync(_ => DataService.DeleteTabAsync(TabId.Value)).Subscribe();
        }
    }
}
