using Fluens.AppCore.Contracts;
using Fluens.AppCore.Enums;
using Fluens.AppCore.ViewModels;
using Microsoft.UI.Xaml;
using ReactiveUI;
using System.Reactive.Linq;
using WinRT;

namespace Fluens.UI;

public sealed partial class MainWindow : Window, IViewFor<MainWindowViewModel>
{
    public ITabPage TabPage => Page;

    public MainWindowViewModel? ViewModel { get; set; }
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = value.As<MainWindowViewModel>(); }

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;

        Observable.FromEventPattern<RoutedEventArgs>(Page, nameof(Page.Loaded))
            .Subscribe(ep => SetTitleBar(Page.TitleBar));

        Page.HasNoTabs.Subscribe(_ => Close());
    }

    public async Task ApplyOnStartupSettingAsync(OnStartupSetting onStartupSetting)
    {
        await Page.ApplyOnStartupSettingAsync(onStartupSetting);
    }
}
