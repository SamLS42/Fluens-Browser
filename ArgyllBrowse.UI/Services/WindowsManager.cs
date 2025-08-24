using ArgyllBrowse.AppCore.Contracts;
using ArgyllBrowse.AppCore.Enums;
using ArgyllBrowse.AppCore.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Reactive.Linq;


namespace ArgyllBrowse.UI.Services;

public class WindowsManager(ILocalSettingService localSettingService, TabPersistencyService dataService)
{
    public MainWindow CreateWindow()
    {
        MainWindow newWindow = new()
        {
            SystemBackdrop = new MicaBackdrop()
        };
        TrackWindow(newWindow);
        return newWindow;
    }

    public MainWindow CreateUnTrackedWindow()
    {
        MainWindow newWindow = new()
        {
            SystemBackdrop = new MicaBackdrop()
        };
        return newWindow;
    }

    public void TrackWindow(Window window)
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
                               await ClearOpenTabsAsync();
                           }
                       });
               }
           });

        ActiveWindows.Add(window);
    }

    private async Task ClearOpenTabsAsync()
    {
        await dataService.ClearOpenTabsAsync();
    }

    public Window? GetWindowForElement(UIElement element)
    {
        return ActiveWindows.FirstOrDefault(w => element.XamlRoot == w.Content.XamlRoot);
    }

    private List<Window> ActiveWindows { get; } = [];
}
