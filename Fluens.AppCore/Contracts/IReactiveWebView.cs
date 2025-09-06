using Fluens.AppCore.Helpers;
using System.Reactive;

namespace Fluens.AppCore.Contracts;

public interface IReactiveWebView : IDisposable
{
    IObservable<string> DocumentTitle { get; }
    IObservable<string> FaviconUrl { get; }
    IObservable<Unit> NavigationCompleted { get; }
    IObservable<Unit> NavigationStarting { get; }
    IObservable<bool> IsNavigating { get; }
    IObservable<Uri> Url { get; }
    IObservable<Uri> OpenNewTab { get; }
    IObservable<ShortcutMessage> KeyboardShortcuts { get; }
    void GoBack();
    void GoForward();
    void StopNavigation();
    void Refresh();
    void NavigateToUrl(Uri url);
    void Setup(string? documentTitle = null, string? faviconUrl = null, Uri? url = null);
}