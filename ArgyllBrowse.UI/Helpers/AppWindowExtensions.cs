using ArgyllBrowse.UI.Enums;
using System;
using Windows.Storage;

namespace ArgyllBrowse.UI.Helpers;

internal static class AppWindowExtensions
{
    private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

    internal static void ApplyOnStartupSetting(this AppWindow window)
    {
        OnStartupSetting onStartupSetting = OnStartupSetting.OpenNewTab;

        if (localSettings.Values.TryGetValue("OnStartupSetting", out object? rawSetting))
        {
            if (Enum.TryParse(rawSetting.ToString(), out OnStartupSetting parsedValue))
            {
                onStartupSetting = parsedValue;
            }
        }

        window.ApplyOnStartupSetting(onStartupSetting);
    }
}