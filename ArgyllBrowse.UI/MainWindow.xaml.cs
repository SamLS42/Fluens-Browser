using ArgyllBrowse.AppCore.Enums;
using Microsoft.UI.Xaml;
using System.Reactive.Linq;

namespace ArgyllBrowse.UI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;

        Observable.FromEventPattern<RoutedEventArgs>(Page, nameof(Page.Loaded))
            .Subscribe(ep => SetTitleBar(Page.TitleBar));

        Page.ViewModel!.HasNoTabs.Subscribe(_ => Close());
    }

    internal void ApplyOnStartupSetting(OnStartupSetting onStartupSetting)
    {
        Page.ApplyOnStartupSetting(onStartupSetting);
    }
}
