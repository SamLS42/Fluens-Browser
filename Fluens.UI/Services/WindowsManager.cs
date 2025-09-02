using Fluens.AppCore.Contracts;
using Fluens.AppCore.Enums;
using Fluens.AppCore.Services;
using Fluens.AppCore.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Reactive.Linq;


namespace Fluens.UI.Services;

public class WindowsManager(ILocalSettingService localSettingService, TabPersistencyService dataService) : IWindowsManager
{
    public IMainWindow CreateWindow()
    {
        MainWindow newWindow = new()
        {
            SystemBackdrop = new MicaBackdrop()
        };
        TrackWindow(newWindow);
        return newWindow;
    }

    private void TrackWindow(MainWindow window)
    {
        Observable.FromEventPattern<object, WindowEventArgs>(window, nameof(window.Closed))
           .Subscribe(ep =>
           {
               ActiveWindows.Remove(window);

               if (ActiveWindows.Count == 0)
               {
                   localSettingService.OnStartupSettingChanges.Take(1)
                       .Subscribe(async onStartupSetting =>
                       {
                           if (onStartupSetting == OnStartupSetting.OpenNewTab)
                           {
                               await dataService.ClearOpenTabsAsync();
                           }
                       });
               }
           });

        ActiveWindows.Add(window);
    }

    //public MainWindow? GetWindowForElement(UIElement element)
    //{
    //    return ActiveWindows.FirstOrDefault(w => element.XamlRoot == w.Content.XamlRoot);
    //}

    public ITabView GetParentTabView(AppTabViewModel tab)
    {
        MainWindow? window = ActiveWindows.SingleOrDefault(window => window.TabView.HasTab(tab));

        return window != null
            ? window.TabView
            : throw new NotSupportedException("All tabs must have an active parent ITabView and MainWindow");
    }

    private List<MainWindow> ActiveWindows { get; } = [];
}
