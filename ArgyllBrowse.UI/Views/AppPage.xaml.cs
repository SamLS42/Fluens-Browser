using ArgyllBrowse.AppCore.Enums;
using ArgyllBrowse.AppCore.Helpers;
using ArgyllBrowse.AppCore.ViewModels;
using ArgyllBrowse.Data.Entities;
using ArgyllBrowse.UI.Helpers;
using ArgyllBrowse.UI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using WinRT;

namespace ArgyllBrowse.UI.Views;

public sealed partial class AppPage : ReactiveAppPage, IDisposable
{
    private readonly CompositeDisposable disposables = [];
    private WindowsManager WindowsManager { get; } = ServiceLocator.GetRequiredService<WindowsManager>();
    private bool isLoaded;

    public AppPage()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<AppPageViewModel>();

        Observable.FromEventPattern<RoutedEventArgs>(this, nameof(Loaded))
            .Subscribe(_ => SetTitleBar());

        Observable.FromEventPattern<TabView, object>(tabView, nameof(tabView.AddTabButtonClick))
            .Subscribe(_ => tabView.AddEmptyTab());

        Observable.FromEventPattern<SelectionChangedEventArgs>(tabView, nameof(tabView.SelectionChanged))
            .Subscribe(ep =>
            {
                ep.EventArgs.RemovedItems.FirstOrDefault()?.As<TabViewItem>().Content.As<AppTab>().ViewModel?.IsTabSelected = false;
                ep.EventArgs.AddedItems.FirstOrDefault()?.As<TabViewItem>().Content.As<AppTab>().ViewModel?.IsTabSelected = true;
            });

        Observable.FromEventPattern<SelectionChangedEventArgs>(tabView, nameof(tabView.SelectionChanged))
            .Select(ep => tabView.SelectedItem)
            .WhereNotNull()
            .SelectMany(vm => vm.As<TabViewItem>().Content.As<AppTab>().ViewModel!.OpenConfig)
            .Subscribe(_ => SettingsDialog.IsOpen = !SettingsDialog.IsOpen)
            .DisposeWith(disposables);

        Observable.FromEventPattern<TabView, TabViewTabCloseRequestedEventArgs>(tabView, nameof(tabView.TabCloseRequested))
            .Subscribe(ep => CloseTab(ep.EventArgs));

        Observable.FromEventPattern<TabView, IVectorChangedEventArgs>(tabView, nameof(tabView.TabItemsChanged))
            .SkipWhile(_ => isLoaded is false)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
            .Subscribe(ep => UpdateTabIndexes());

        Observable.FromEventPattern<SizeChangedEventArgs>(tabView, nameof(tabView.SizeChanged))
            .Subscribe(ep =>
            {
                SettingsDialogContent.Width = tabView.ActualWidth - (2 * SettingsDialogContent.Margin.Left);
                SettingsDialogContent.Height = tabView.ActualHeight - (2 * SettingsDialogContent.Margin.Top);
            });
    }

    private void SetTitleBar()
    {
        MainWindow currentWindow = WindowsManager.GetWindowForElement(this)!;
        currentWindow.SetTitleBar(CustomDragRegion);
        CustomDragRegion.MinWidth = 188;
    }

    private void CloseTab(TabViewTabCloseRequestedEventArgs eventArgs)
    {
        RemoveTab(eventArgs.Tab);
    }

    private void RemoveTab(TabViewItem tabViewItem)
    {
        AppTab tab = tabViewItem.Content.As<AppTab>();

        tabView.TabItems.Remove(tabViewItem);

        if (tabView.TabItems.Count == 0)
        {
            Window? window = WindowsManager.GetWindowForElement(this);
            window?.Close();
        }

        tab.Dispose();
    }
    private void UpdateTabIndexes()
    {
        foreach (TabViewItem tabItem in tabView.TabItems.OfType<TabViewItem>())
        {
            AppTab appTab = tabItem.Content.As<AppTab>();
            int newIndex = tabView.TabItems.IndexOf(tabItem);
            appTab.ViewModel?.Index = newIndex;
        }
    }

    private async Task RestoreOpenTabs()
    {
        BrowserTab[] tabs = await ViewModel!.GetOpenTabsAsync();

        foreach (BrowserTab item in tabs)
        {
            tabView.AddAppTab(item);
        }
    }

    internal void ApplyOnStartupSetting(OnStartupSetting onStartupSetting)
    {
        Observable.FromEventPattern<RoutedEventArgs>(tabView, nameof(tabView.Loaded))
            .Subscribe(async _ =>
            {
                switch (onStartupSetting)
                {
                    case OnStartupSetting.OpenNewTab:
                        tabView.AddEmptyTab();
                        break;
                    case OnStartupSetting.RestoreOpenTabs:
                        await RestoreOpenTabs();
                        break;
                    //TODO
                    //case OnStartupSetting.OpenSpecificTabs:
                    //    break;
                    case OnStartupSetting.RestoreAndOpenNewTab:
                        await RestoreOpenTabs();
                        tabView.AddEmptyTab();
                        break;
                    default:
                        tabView.AddEmptyTab();
                        break;
                }

                if (tabView.TabItems.Count == 0)
                {
                    tabView.AddEmptyTab();
                }

                tabView.SelectedItem ??= tabView.TabItems.First();

                isLoaded = true;

            });
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}

public partial class ReactiveAppPage : ReactiveUserControl<AppPageViewModel>;