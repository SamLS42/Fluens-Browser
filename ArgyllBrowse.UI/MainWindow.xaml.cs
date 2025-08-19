using ArgyllBrowse.AppCore.Enums;
using Microsoft.UI.Xaml;

namespace ArgyllBrowse.UI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
    }

    internal void ApplyOnStartupSetting(OnStartupSetting onStartupSetting)
    {
        Page.ApplyOnStartupSetting(onStartupSetting);
    }
}
