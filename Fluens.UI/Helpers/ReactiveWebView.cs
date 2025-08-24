using Fluens.AppCore.Contracts;
using Fluens.AppCore.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Fluens.UI.Helpers;
public sealed partial class ReactiveWebView : IReactiveWebView
{
    private readonly CompositeDisposable Disposables = [];
    public required WebView2 MyWebView { get; set; }
    public BehaviorSubject<bool> IsLoading { get; } = new(false);
    public BehaviorSubject<string> DocumentTitle { get; private set; } = null!;
    public BehaviorSubject<string> FaviconUrl { get; private set; } = null!;
    public Subject<Unit> NavigationStarting { get; } = new();
    public Subject<Unit> NavigationCompleted { get; } = new();
    public BehaviorSubject<Uri> Url { get; private set; } = null!;

    public void Setup(string? documentTitle = null, string? faviconUrl = null, Uri? url = null)
    {
        DocumentTitle = new(documentTitle ?? string.Empty);
        FaviconUrl = new(faviconUrl ?? string.Empty);
        Url = new(url ?? Constants.AboutBlankUri);

        Observable.FromEventPattern<WebView2, CoreWebView2InitializedEventArgs>(MyWebView, nameof(MyWebView.CoreWebView2Initialized)).Subscribe(ep =>
        {
            Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationStarting))
                .Select(_ => true)
                .Merge(Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationCompleted))
                .Select(_ => false))
                .Subscribe(v => IsLoading.OnNext(v));

            Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.DocumentTitleChanged))
                .Select(_ => MyWebView.CoreWebView2.DocumentTitle)
                .Subscribe(DocumentTitle.OnNext)
                .DisposeWith(Disposables);

            Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationStarting))
                .Subscribe(_ =>
                {
                    NavigationStarting.OnNext(Unit.Default);
                    FaviconUrl.OnNext(UIConstants.LoadingFaviconUri);
                })
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, object>(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.FaviconChanged))
                .Subscribe(_ => FaviconUrl.OnNext(MyWebView.CoreWebView2.FaviconUri))
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2NavigationCompletedEventArgs>(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationCompleted))
                .Subscribe(ep =>
                {
                    if (ep.EventArgs.IsSuccess)
                    {
                        NavigationCompleted.OnNext(Unit.Default);

                        if (Url.Value == Constants.AboutBlankUri)
                        {
                            FaviconUrl.OnNext(string.Empty);
                        }
                    }
                })
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
        DocumentTitle.OnCompleted();
        NavigationStarting.OnCompleted();
        NavigationCompleted.OnCompleted();
        Url.OnCompleted();
        Disposables.Dispose();
        MyWebView.Close();
    }

    public void NavigateToUrl(Uri url)
    {
        MyWebView.Source = url;
    }
}
