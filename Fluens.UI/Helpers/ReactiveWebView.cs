using Fluens.AppCore.Contracts;
using Fluens.AppCore.Helpers;
using Fluens.UI.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;

namespace Fluens.UI.Helpers;
public sealed partial class ReactiveWebView : IReactiveWebView
{
    private readonly CompositeDisposable Disposables = [];
    public required WebView2 MyWebView { get; set; }
    private BehaviorSubject<bool> IsLoadingSource { get; } = new(false);
    public IObservable<bool> IsLoading => IsLoadingSource.AsObservable();
    private BehaviorSubject<string> DocumentTitleSource { get; set; } = null!;
    public IObservable<string> DocumentTitle => DocumentTitleSource.AsObservable();
    private BehaviorSubject<string> FaviconUrlSource { get; set; } = null!;
    public IObservable<string> FaviconUrl => FaviconUrlSource.AsObservable();
    private Subject<Unit> NavigationStartingSource { get; } = new();
    public IObservable<Unit> NavigationStarting => NavigationStartingSource.AsObservable();
    private Subject<Unit> NavigationCompletedSource { get; } = new();
    public IObservable<Unit> NavigationCompleted => NavigationCompletedSource.AsObservable();
    private BehaviorSubject<Uri> UrlSource { get; set; } = null!;
    public IObservable<Uri> Url => UrlSource.AsObservable();
    private Subject<Uri> OpenNewTabSource { get; set; } = new();
    public IObservable<Uri> OpenNewTab => OpenNewTabSource.AsObservable();
    private Subject<ShortcutMessage> KeyboardShortcutsSource { get; set; } = new();
    public IObservable<ShortcutMessage> KeyboardShortcuts => KeyboardShortcutsSource.AsObservable();

    public void Setup(string? documentTitle = null, string? faviconUrl = null, Uri? url = null)
    {
        DocumentTitleSource = new(documentTitle ?? string.Empty);
        FaviconUrlSource = new(faviconUrl ?? string.Empty);
        UrlSource = new(url ?? Constants.AboutBlankUri);

        Observable.FromEventPattern<WebView2, CoreWebView2InitializedEventArgs>(MyWebView, nameof(MyWebView.CoreWebView2Initialized)).Subscribe(async ep =>
        {
            await AddShortcutListenersAsync();

            Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationStarting))
                .Select(_ => true)
                .Merge(Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationCompleted))
                .Select(_ => false))
                .Subscribe(v => IsLoadingSource.OnNext(v));

            Observable.FromEventPattern(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.DocumentTitleChanged))
                .Select(_ => MyWebView.CoreWebView2.DocumentTitle)
                .Subscribe(DocumentTitleSource.OnNext)
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2NavigationStartingEventArgs>(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationStarting))
                .Subscribe(ep =>
                {
                    NavigationStartingSource.OnNext(Unit.Default);
                    FaviconUrlSource.OnNext(UIConstants.LoadingFaviconUri);
                })
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, object>(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.FaviconChanged))
                .Select(_ => MyWebView.CoreWebView2.FaviconUri)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Subscribe(x => FaviconUrlSource.OnNext(x))
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2NavigationCompletedEventArgs>(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NavigationCompleted))
                .Subscribe(async ep =>
                {
                    await AddShortcutListenersAsync();

                    if (ep.EventArgs.IsSuccess)
                    {
                        NavigationCompletedSource.OnNext(Unit.Default);

                        if (MyWebView.Source == Constants.AboutBlankUri)
                        {
                            FaviconUrlSource.OnNext(string.Empty);
                        }
                    }
                })
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2SourceChangedEventArgs>(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.SourceChanged))
                .Subscribe(_ => UrlSource.OnNext(MyWebView.Source))
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2NewWindowRequestedEventArgs>(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.NewWindowRequested))
                .Subscribe(ep =>
                {
                    ep.EventArgs.Handled = true;

                    if (Uri.TryCreate(ep.EventArgs.Uri, UriKind.Absolute, out Uri? uri))
                    {
                        OpenNewTabSource.OnNext(uri);
                    }
                })
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2WebMessageReceivedEventArgs>(MyWebView.CoreWebView2, nameof(MyWebView.CoreWebView2.WebMessageReceived))
                .Subscribe(ep =>
                {
                    ShortcutMessage? message = JsonSerializer.Deserialize<ShortcutMessage>(ep.EventArgs.WebMessageAsJson);

                    if (message == null)
                    {
                        Log_ShortcutMessage_Deserialization_Error(ep.EventArgs.WebMessageAsJson);
                        return;
                    }

                    KeyboardShortcutsSource.OnNext(message);
                })
                .DisposeWith(Disposables);

        }).DisposeWith(Disposables);
    }

    private async Task AddShortcutListenersAsync()
    {
        string script = @"
                        window.addEventListener('keydown', function(e) {
                            if (e.ctrlKey && e.key === 't') {
                                e.preventDefault();
                                window.chrome.webview.postMessage({ key: 'T', ctrl: true });
                            }
                            if (e.ctrlKey && e.key === 'w') {
                                e.preventDefault();
                                window.chrome.webview.postMessage({ key: 'W', ctrl: true });
                            }
                        });
                    ";

        await MyWebView.CoreWebView2.ExecuteScriptAsync(script);
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
        IsLoadingSource.OnCompleted();
        DocumentTitleSource.OnCompleted();
        NavigationStartingSource.OnCompleted();
        NavigationCompletedSource.OnCompleted();
        UrlSource.OnCompleted();
        OpenNewTabSource.OnCompleted();
        KeyboardShortcutsSource.OnCompleted();
        Disposables.Dispose();
        MyWebView.Close();
    }

    public void NavigateToUrl(Uri url)
    {
        MyWebView.Source = url;
    }


    //Used by the LoggerMessage
#pragma warning disable CA1823 // Avoid unused private fields
    private readonly ILogger _logger = ServiceLocator.GetRequiredService<ILogger<ReactiveWebView>>();
#pragma warning restore CA1823 // Avoid unused private fields

    [LoggerMessage(Level = LogLevel.Error, Message = "ShortcutMessage Deserialization Error: {webMessageAsJson}")]
    private partial void Log_ShortcutMessage_Deserialization_Error(string webMessageAsJson);
}
