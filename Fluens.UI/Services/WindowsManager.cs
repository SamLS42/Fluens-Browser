using Fluens.AppCore.Contracts;
using Fluens.AppCore.Enums;
using Fluens.AppCore.Services;
using Fluens.AppCore.ViewModels;
using Fluens.UI.Views;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Reactive.Linq;


namespace Fluens.UI.Services;

public class WindowsManager(ILocalSettingService localSettingService, TabPersistencyService TabPersistencyService, BrowserWindowService browserWindowService)
{
    public MainWindow CreateWindow()
    {
        MainWindow newWindow = new()
        {
            SystemBackdrop = new MicaBackdrop(),
        };
        TrackWindow(newWindow);
        return newWindow;
    }

    private void TrackWindow(MainWindow window)
    {
        Observable.FromEventPattern<object, WindowEventArgs>(window, nameof(window.Closed))
           .Subscribe(async ep =>
           {
               ActiveWindows.Remove(window);

               AppWindow appWindow = window.AppWindow;

               int id = window.ViewModel!.Id;
               int x = appWindow.Position.X;
               int y = appWindow.Position.Y;
               int width = appWindow.Size.Width;
               int height = appWindow.Size.Height;
               bool isMaximized = appWindow.Presenter.Kind == AppWindowPresenterKind.Overlapped &&
                    ((OverlappedPresenter)appWindow.Presenter).State == OverlappedPresenterState.Maximized;

               await browserWindowService.SaveWindowStateAsync(id, x, y, width, height, isMaximized);

               if (ActiveWindows.Count == 0)
               {
                   ApplyPersistencySettings();
               }
           });

        ActiveWindows.Add(window);
    }

    private void ApplyPersistencySettings()
    {
        localSettingService.OnStartupSettingChanges.Take(1)
            .Subscribe(async onStartupSetting =>
            {
                if (onStartupSetting is not OnStartupSetting.RestoreOpenTabs and not OnStartupSetting.RestoreAndOpenNewTab)
                {
                    await TabPersistencyService.ClearTabsAsync();
                    await browserWindowService.ClearWindowsAsync();
                    return;
                }
            });
    }

    //public MainWindow? GetWindowForElement(UIElement element)
    //{
    //    return ActiveWindows.FirstOrDefault(w => element.XamlRoot == w.Content.XamlRoot);
    //}

    public ITabPage GetParentTabPage(AppTabViewModel tab)
    {
        MainWindow? window = ActiveWindows.SingleOrDefault(window => window.TabPage.HasTab(tab));

        return window != null
            ? window.TabPage
            : throw new NotSupportedException("All tabs must have an active parent ITabView and MainWindow");
    }

    public int GetParentWindowId(AppPage page)
    {
        return ActiveWindows.FirstOrDefault(w => page.XamlRoot == w.Content.XamlRoot)?.ViewModel!.Id
            ?? throw new NotSupportedException("All MainWindows must have an Id");
    }

    private List<MainWindow> ActiveWindows { get; } = [];
}
