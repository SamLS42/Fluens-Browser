using Fluens.AppCore.Contracts;
using Fluens.AppCore.Helpers;
using Fluens.UI.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;

namespace Fluens.UI.Helpers;

public sealed partial class ObservableWebView : IObservableWebView
{
    private readonly CompositeDisposable Disposables = [];
    private readonly WebView2 WebView;
    private Subject<bool> IsNavigatingSource { get; } = new();
    public IObservable<bool> IsNavigating => IsNavigatingSource.AsObservable();
    private Subject<string> DocumentTitleSource { get; set; } = new();
    public IObservable<string> DocumentTitle => DocumentTitleSource.AsObservable();
    private Subject<string> FaviconUrlSource { get; set; } = new();
    public IObservable<string> FaviconUrl => FaviconUrlSource.AsObservable();
    private Subject<Uri> UrlSource { get; set; } = new();
    public IObservable<Uri> Url => UrlSource.AsObservable();
    private Subject<Uri> OpenNewTabSource { get; set; } = new();
    public IObservable<Uri> OpenNewTab => OpenNewTabSource.AsObservable();
    private Subject<ShortcutMessage> KeyboardShortcutsSource { get; set; } = new();
    public IObservable<ShortcutMessage> KeyboardShortcuts => KeyboardShortcutsSource.AsObservable();
    private BehaviorSubject<bool> InitializedSource { get; set; } = new(false);
    public IObservable<bool> Initialized => InitializedSource.AsObservable();

    public Uri? Source => WebView.Source;

    public ObservableWebView(WebView2 webView)
    {
        WebView = webView;

        Observable.FromEventPattern<WebView2, CoreWebView2InitializedEventArgs>(WebView, nameof(WebView.CoreWebView2Initialized))
            .Subscribe(async ep =>
        {
            InitializedSource.OnNext(true);
            await AddShortcutListenersAsync();

            Observable.FromEventPattern(WebView.CoreWebView2, nameof(WebView.CoreWebView2.NavigationStarting))
                .Select(_ => true)
                .Merge(Observable.FromEventPattern(WebView.CoreWebView2, nameof(WebView.CoreWebView2.NavigationCompleted))
                .Select(_ => false))
                .Subscribe(v => IsNavigatingSource.OnNext(v));

            Observable.FromEventPattern(WebView.CoreWebView2, nameof(WebView.CoreWebView2.DocumentTitleChanged))
                .Select(_ => WebView.CoreWebView2.DocumentTitle)
                .Subscribe(DocumentTitleSource.OnNext)
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2NavigationStartingEventArgs>(WebView.CoreWebView2, nameof(WebView.CoreWebView2.NavigationStarting))
                .Subscribe(ep =>
                {
                    FaviconUrlSource.OnNext(Constants.LoadingFaviconUri);
                })
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, object>(WebView.CoreWebView2, nameof(WebView.CoreWebView2.FaviconChanged))
                .Select(_ => WebView.CoreWebView2.FaviconUri)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Subscribe(x => FaviconUrlSource.OnNext(x))
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2NavigationCompletedEventArgs>(WebView.CoreWebView2, nameof(WebView.CoreWebView2.NavigationCompleted))
                .Subscribe(async ep =>
                {
                    await AddShortcutListenersAsync();

                    if (ep.EventArgs.IsSuccess)
                    {
                        if (WebView.Source == Constants.AboutBlankUri)
                        {
                            FaviconUrlSource.OnNext(string.Empty);
                        }
                        else
                        {
                            FaviconUrlSource.OnNext(WebView.CoreWebView2.FaviconUri);
                        }
                    }
                })
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, object>(WebView.CoreWebView2, nameof(WebView.CoreWebView2.HistoryChanged))
                .Select(ep => WebView.Source)
                .Subscribe(UrlSource.OnNext)
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2NewWindowRequestedEventArgs>(WebView.CoreWebView2, nameof(WebView.CoreWebView2.NewWindowRequested))
                .Subscribe(ep =>
                {
                    ep.EventArgs.Handled = true;

                    if (Uri.TryCreate(ep.EventArgs.Uri, UriKind.Absolute, out Uri? uri))
                    {
                        OpenNewTabSource.OnNext(uri);
                    }
                })
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2WebMessageReceivedEventArgs>(WebView.CoreWebView2, nameof(WebView.CoreWebView2.WebMessageReceived))
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

        })
            .DisposeWith(Disposables);
    }

    private async Task AddShortcutListenersAsync()
    {
        string script = @"
window.addEventListener('keydown', function (e) {
  const combo = `${e.code}|ctrl:${e.ctrlKey }|shift:${e.shiftKey}`;
  switch (combo) {
    case 'KeyT|ctrl:true|shift:true':
      e.preventDefault();
      window.chrome.webview.postMessage({ key: 'T', ctrl: true, shift: true });
      break;

    case 'KeyT|ctrl:true|shift:false':
      e.preventDefault();
      window.chrome.webview.postMessage({ key: 'T', ctrl: true, shift: false });
      break;

    case 'KeyW|ctrl:true|shift:false':
    case 'KeyW|ctrl:true|shift:true': // handle with same action or split if needed
      e.preventDefault();
      window.chrome.webview.postMessage({ key: 'W', ctrl: true, shift: e.shiftKey });
      break;
  }
});
";

        await WebView.CoreWebView2.ExecuteScriptAsync(script);
    }

    //TODO: this stop doesn't work on SPA, i.e. Youtube, it doens't stops scripts
    //https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.stop?view=webview2-dotnet-1.0.3351.48#remarks
    public void StopNavigation()
    {
        WebView.CoreWebView2.Stop();
    }

    public void Refresh()
    {
        WebView.CoreWebView2.Reload();
    }

    public void Dispose()
    {
        IsNavigatingSource.OnCompleted();
        DocumentTitleSource.OnCompleted();
        UrlSource.OnCompleted();
        OpenNewTabSource.OnCompleted();
        KeyboardShortcutsSource.OnCompleted();
        Disposables.Dispose();
        WebView.Close();
    }

    public void NavigateToUrl(Uri url)
    {
        Observable.FromAsync(async _ => await WebView.EnsureCoreWebView2Async()).Subscribe();
        InitializedSource.Where(i => i).Take(1).Subscribe(e => WebView.Source = url);
    }


    //Used by the LoggerMessage
#pragma warning disable CA1823 // Avoid unused private fields
    private readonly ILogger _logger = ServiceLocator.GetRequiredService<ILogger<ObservableWebView>>();
#pragma warning restore CA1823 // Avoid unused private fields

    [LoggerMessage(Level = LogLevel.Error, Message = "ShortcutMessage Deserialization Error: {webMessageAsJson}")]
    private partial void Log_ShortcutMessage_Deserialization_Error(string webMessageAsJson);

    public void GoBack()
    {
        WebView.GoBack();
    }

    public void GoForward()
    {
        WebView.GoForward();
    }

    public async Task ActivateAsync()
    {
        await WebView.EnsureCoreWebView2Async();
    }
}
