using System;
using System.Reactive;
using System.Reactive.Subjects;

namespace ArgyllBrowse.UI.ViewModels.Contracts;
public interface IReactiveWebView : IDisposable
{
    Subject<string> DocumentTitleChanges { get; }
    BehaviorSubject<string> FaviconUrlChanges { get; }
    Subject<Unit> NavigationCompleted { get; }
    Subject<Unit> NavigationStarting { get; }
    BehaviorSubject<bool> IsLoading { get; }
    Subject<Uri> Url { get; }
    void GoBack();
    void GoForward();
    void StopNavigation();
    void Refresh();
    void NavigateToUrl(Uri url);
}