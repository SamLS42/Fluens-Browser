using Fluens.AppCore.Enums;
using Fluens.AppCore.ViewModels;
using Microsoft.UI.Xaml;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using WinRT;

namespace Fluens.UI;

public sealed partial class MainWindow : Window, IViewFor<MainWindowViewModel>
{
    readonly CompositeDisposable disposables = [];
    public IViewFor<AppPageViewModel> TabPage => Page;

    public MainWindowViewModel? ViewModel { get; set; }
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = value.As<MainWindowViewModel>(); }

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;

        Observable.FromEventPattern<RoutedEventArgs>(Page, nameof(Page.Loaded))
            .Subscribe(ep => SetTitleBar(Page.TitleBar))
            .DisposeWith(disposables);

        Page.ViewModel!.HasNoTabs
            .Subscribe(_ => Close())
            .DisposeWith(disposables);

        Observable.FromEventPattern<WindowEventArgs>(this, nameof(Closed))
            .Subscribe(ep => disposables.Dispose());
    }

    public async Task ApplyOnStartupSettingAsync(OnStartupSetting onStartupSetting)
    {
        Page.ViewModel?.WindowId = ViewModel!.Id;
        await Page.ViewModel!.ApplyOnStartupSettingAsync(onStartupSetting);
    }
}
