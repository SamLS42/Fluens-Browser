using ArgyllBrowse.UI.Enums;
using ArgyllBrowse.UI.Views;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WindowManagement;

namespace ArgyllBrowse.UI;

public sealed partial class AppWindow : Window
{
    public AppWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
    }

    internal void ApplyOnStartupSetting(OnStartupSetting onStartupSetting)
    {
        switch (onStartupSetting)
        {
            case OnStartupSetting.OpenNewTab:
                Page.AddNewTab();
                break;
            case OnStartupSetting.RestoreOpenTabs:
                Page.ViewModel!.RestoreOpenTabs();
                break;
            case OnStartupSetting.OpenSpecificTabs:
                break;
            case OnStartupSetting.RestoreAndOpenNewTab:
                break;
            default:
                Page.AddNewTab();
                break;
        }
    }
}
