using Fluens.AppCore.Enums;
using Microsoft.UI.Xaml;
using System.Reactive.Linq;

namespace Fluens.UI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;

        Observable.FromEventPattern<RoutedEventArgs>(Page, nameof(Page.Loaded))
            .Subscribe(ep => SetTitleBar(Page.TitleBar));

        Page.HasNoTabs.Subscribe(_ => Close());
    }

    internal async Task ApplyOnStartupSettingAsync(OnStartupSetting onStartupSetting)
    {
        await Page.ApplyOnStartupSettingAsync(onStartupSetting);
    }
}
