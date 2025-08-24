using System.Reactive;
using System.Reactive.Subjects;

namespace ArgyllBrowse.AppCore.Contracts;
public interface IReactiveWebView : IDisposable
{
    BehaviorSubject<string> DocumentTitle { get; }
    BehaviorSubject<string> FaviconUrl { get; }
    Subject<Unit> NavigationCompleted { get; }
    Subject<Unit> NavigationStarting { get; }
    BehaviorSubject<bool> IsLoading { get; }
    BehaviorSubject<Uri> Url { get; }
    void GoBack();
    void GoForward();
    void StopNavigation();
    void Refresh();
    void NavigateToUrl(Uri url);
    void Setup(string? documentTitle = null, string? faviconUrl = null, Uri? url = null);
}