using ArgyllBrowse.Data.Services;
using ArgyllBrowse.UI.Enums;
using ArgyllBrowse.UI.ViewModels.Contracts;
using DynamicData;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;                        // for Action<T>
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WinRT;


namespace ArgyllBrowse.UI.Services;

public class WindowsManager(ILocalSettingService localSettingService, BrowserDataService dataService)
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

    private void TrackWindow(MainWindow window)
    {
        Observable.FromEventPattern<object, WindowEventArgs>(window, nameof(window.Closed))
           .Subscribe(ep =>
           {
               ActiveWindows.Remove(ep.Sender.As<MainWindow>());

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

    public MainWindow? GetWindowForElement(UIElement element)
    {
        return ActiveWindows.FirstOrDefault(w => element.XamlRoot == w.Content.XamlRoot);
    }

    private List<MainWindow> ActiveWindows { get; } = [];
}
