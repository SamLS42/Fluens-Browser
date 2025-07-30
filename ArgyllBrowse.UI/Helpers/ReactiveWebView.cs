using ArgyllBrowse.UI.ViewModels.Contracts;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ArgyllBrowse.UI.Helpers;
internal sealed partial class ReactiveWebView : IReactiveWebView
{
    private readonly CompositeDisposable Disposables = [];
    private WebView2 MyWebView { get; } = null!;
    public BehaviorSubject<bool> IsLoading { get; } = new(false);
    public Subject<string> DocumentTitleChanges { get; } = new();
    public BehaviorSubject<string> FaviconUrlChanges { get; } = new(string.Empty);
    public Subject<Unit> NavigationStarting { get; } = new();
    public Subject<Unit> NavigationCompleted { get; } = new();
    public Subject<Uri> Url { get; } = new();

    public ReactiveWebView(WebView2 webView)
    {
        MyWebView = webView;

        Observable.FromEventPattern(MyWebView, nameof(MyWebView.CoreWebView2Initialized)).Subscribe(_ =>
        {
            Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationStarting))
                .Select(_ => true)
                .Merge(Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationCompleted))
                .Select(_ => false))
                .Subscribe(v => IsLoading.OnNext(v));

            Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.DocumentTitleChanged))
                .Select(_ => MyWebView.CoreWebView2.DocumentTitle)
                .Subscribe(DocumentTitleChanges.OnNext)
                .DisposeWith(Disposables);

            Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.FaviconChanged))
                .Select(_ => MyWebView.CoreWebView2.FaviconUri)
                .Subscribe(FaviconUrlChanges.OnNext)
                .DisposeWith(Disposables);

            Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationStarting))
                .Subscribe(_ => NavigationStarting.OnNext(Unit.Default))
                .DisposeWith(Disposables);

            Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationCompleted))
                .Subscribe(_ => NavigationCompleted.OnNext(Unit.Default))
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2SourceChangedEventArgs>(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.SourceChanged))
                .Subscribe(_ => Url.OnNext(MyWebView.Source))
                .DisposeWith(Disposables);
        }).DisposeWith(Disposables);
    }

    public void GoBack()
    {
        MyWebView.GoBack();
    }

    public void GoForward()
    {
        MyWebView.GoForward();
    }

    //TODO: this stop doesn't work on SPA, i.e. Youtube, it doens't stops scripts
    //https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.stop?view=webview2-dotnet-1.0.3351.48#remarks
    public void StopNavigation()
    {
        MyWebView.CoreWebView2.Stop();
    }

    public void Refresh()
    {
        MyWebView.CoreWebView2.Reload();
    }

    public void Dispose()
    {
        IsLoading.OnCompleted();
        DocumentTitleChanges.OnCompleted();
        FaviconUrlChanges.OnCompleted();
        NavigationStarting.OnCompleted();
        NavigationCompleted.OnCompleted();
        Url.OnCompleted();
        Disposables.Dispose();
    }

    public void NavigateToUrl(Uri url)
    {
        MyWebView.Source = url;
    }
}
