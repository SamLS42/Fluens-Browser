using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;

namespace ArgyllBrowse.UI.Services;

public class WindowsManager
{
    public AppWindow CreateWindow()
    {
        AppWindow newWindow = new()
        {
            SystemBackdrop = new MicaBackdrop()
        };
        TrackWindow(newWindow);
        return newWindow;
    }

    private void TrackWindow(AppWindow window)
    {
        window.Closed += (sender, args) =>
        {
            ActiveWindows.Remove(window);
        };
        ActiveWindows.Add(window);
    }

    public AppWindow? GetWindowForElement(UIElement element)
    {
        return ActiveWindows.FirstOrDefault(w => element.XamlRoot == w.Content.XamlRoot);
    }

    private List<AppWindow> ActiveWindows { get; } = [];
}
