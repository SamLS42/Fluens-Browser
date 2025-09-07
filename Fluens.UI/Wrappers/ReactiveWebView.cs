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

public sealed partial class ReactiveWebView : WebView2, IReactiveWebView
{
    private readonly CompositeDisposable Disposables = [];
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

    public ReactiveWebView()
    {
        Observable.FromEventPattern<WebView2, CoreWebView2InitializedEventArgs>(this, nameof(CoreWebView2Initialized)).Subscribe(async ep =>
        {
            await AddShortcutListenersAsync();

            Observable.FromEventPattern(CoreWebView2, nameof(CoreWebView2.NavigationStarting))
                .Select(_ => true)
                .Merge(Observable.FromEventPattern(CoreWebView2, nameof(CoreWebView2.NavigationCompleted))
                .Select(_ => false))
                .Subscribe(v => IsNavigatingSource.OnNext(v));

            Observable.FromEventPattern(CoreWebView2, nameof(CoreWebView2.DocumentTitleChanged))
                .Select(_ => CoreWebView2.DocumentTitle)
                .Subscribe(DocumentTitleSource.OnNext)
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2NavigationStartingEventArgs>(CoreWebView2, nameof(CoreWebView2.NavigationStarting))
                .Subscribe(ep =>
                {
                    FaviconUrlSource.OnNext(Constants.LoadingFaviconUri);
                })
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, object>(CoreWebView2, nameof(CoreWebView2.FaviconChanged))
                .Select(_ => CoreWebView2.FaviconUri)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Subscribe(x => FaviconUrlSource.OnNext(x))
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2NavigationCompletedEventArgs>(CoreWebView2, nameof(CoreWebView2.NavigationCompleted))
                .Subscribe(async ep =>
                {
                    await AddShortcutListenersAsync();

                    if (ep.EventArgs.IsSuccess)
                    {
                        if (Source == Constants.AboutBlankUri)
                        {
                            FaviconUrlSource.OnNext(string.Empty);
                        }
                        else
                        {
                            FaviconUrlSource.OnNext(CoreWebView2.FaviconUri);
                        }
                    }
                })
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, object>(CoreWebView2, nameof(CoreWebView2.HistoryChanged))
                .Select(ep => Source)
                .Subscribe(UrlSource.OnNext)
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2NewWindowRequestedEventArgs>(CoreWebView2, nameof(CoreWebView2.NewWindowRequested))
                .Subscribe(ep =>
                {
                    ep.EventArgs.Handled = true;

                    if (Uri.TryCreate(ep.EventArgs.Uri, UriKind.Absolute, out Uri? uri))
                    {
                        OpenNewTabSource.OnNext(uri);
                    }
                })
                .DisposeWith(Disposables);

            Observable.FromEventPattern<CoreWebView2, CoreWebView2WebMessageReceivedEventArgs>(CoreWebView2, nameof(CoreWebView2.WebMessageReceived))
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

        await CoreWebView2.ExecuteScriptAsync(script);
    }

    //TODO: this stop doesn't work on SPA, i.e. Youtube, it doens't stops scripts
    //https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.stop?view=webview2-dotnet-1.0.3351.48#remarks
    public void StopNavigation()
    {
        CoreWebView2.Stop();
    }

    public void Refresh()
    {
        CoreWebView2.Reload();
    }

    public void Dispose()
    {
        IsNavigatingSource.OnCompleted();
        DocumentTitleSource.OnCompleted();
        UrlSource.OnCompleted();
        OpenNewTabSource.OnCompleted();
        KeyboardShortcutsSource.OnCompleted();
        Disposables.Dispose();
        Close();
    }

    public void NavigateToUrl(Uri url)
    {
        Source = url;
    }


    //Used by the LoggerMessage
#pragma warning disable CA1823 // Avoid unused private fields
    private readonly ILogger _logger = ServiceLocator.GetRequiredService<ILogger<ReactiveWebView>>();
#pragma warning restore CA1823 // Avoid unused private fields

    [LoggerMessage(Level = LogLevel.Error, Message = "ShortcutMessage Deserialization Error: {webMessageAsJson}")]
    private partial void Log_ShortcutMessage_Deserialization_Error(string webMessageAsJson);
}
