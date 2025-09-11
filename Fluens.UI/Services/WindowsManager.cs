using Fluens.AppCore.Contracts;
using Fluens.AppCore.Enums;
using Fluens.AppCore.Services;
using Fluens.AppCore.ViewModels;
using Fluens.UI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using ReactiveUI;
using System.Reactive.Linq;
using Vanara.PInvoke;
using WinRT.Interop;

namespace Fluens.UI.Services;

public class WindowsManager(ILocalSettingService localSettingService, TabPersistencyService TabPersistencyService, BrowserWindowService browserWindowService)
{
    public MainWindow CreateWindow(int id)
    {
        MainWindow newWindow = new()
        {
            SystemBackdrop = new MicaBackdrop(),
            ViewModel = new() { Id = id },
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

               // 1. Get the window's native handle (HWND)
               HWND hwnd = new(WindowNative.GetWindowHandle(window));

               // We'll populate these variables based on the window state
               int x, y, width, height;
               bool isMaximized = false;

               // 2. Get the placement to check if the window is maximized
               User32.WINDOWPLACEMENT placement = new();
               if (User32.GetWindowPlacement(hwnd, ref placement))
               {
                   if (placement.showCmd == ShowWindowCommand.SW_SHOWMAXIMIZED)
                   {
                       // STATE: MAXIMIZED
                       // Save the underlying "normal" position for when the user restores.
                       isMaximized = true;
                       RECT normalRect = placement.rcNormalPosition;
                       x = normalRect.X;
                       y = normalRect.Y;
                       width = normalRect.Width;
                       height = normalRect.Height;
                   }
                   else
                   {
                       // STATE: NORMAL or SNAPPED
                       // Get the window's *actual current* screen coordinates.
                       // This will correctly capture the size and position of a snapped window.
                       if (User32.GetWindowRect(hwnd, out RECT currentRect))
                       {
                           x = currentRect.X;
                           y = currentRect.Y;
                           width = currentRect.Width;
                           height = currentRect.Height;
                       }
                       else
                       {
                           // Fallback or error handling if GetWindowRect fails
                           // For now, we can just return and not save.
                           return;
                       }
                   }

                   // 3. Save the determined state
                   int id = window.ViewModel!.Id;
                   await browserWindowService.SaveWindowStateAsync(id, x, y, width, height, isMaximized);
               }

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

    public IViewFor<AppPageViewModel> GetParentTabPage(AppTabViewModel tab)
    {
        MainWindow? window = ActiveWindows.SingleOrDefault(window => window.TabPage.ViewModel!.HasTab(tab));

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
