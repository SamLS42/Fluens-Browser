using ArgyllBrowse.Data.Entities;
using ArgyllBrowse.UI.Enums;
using ArgyllBrowse.UI.Helpers;
using ArgyllBrowse.UI.Services;
using ArgyllBrowse.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
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
            .Subscribe(_ => SetTitleBar())
            .DisposeWith(disposables);

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<TabView, object>(tabView, nameof(tabView.AddTabButtonClick))
                .Subscribe(_ => tabView.AddNewAppTab()).DisposeWith(d);

            Observable.FromEventPattern<SelectionChangedEventArgs>(tabView, nameof(tabView.SelectionChanged))
                .Subscribe(ep =>
                {
                    ep.EventArgs.RemovedItems.FirstOrDefault()?.As<TabViewItem>().Content.As<ReactiveAppTab>().ViewModel?.IsTabSelected = false;
                    ep.EventArgs.AddedItems.FirstOrDefault()?.As<TabViewItem>().Content.As<ReactiveAppTab>().ViewModel?.IsTabSelected = true;
                }).DisposeWith(d);

            Observable.FromEventPattern<TabView, TabViewTabCloseRequestedEventArgs>(tabView, nameof(tabView.TabCloseRequested))
                .Subscribe(ep => CloseTab(ep.EventArgs)).DisposeWith(d);

            Observable.FromEventPattern<TabView, IVectorChangedEventArgs>(tabView, nameof(tabView.TabItemsChanged))
                .SkipWhile(_ => isLoaded is false)
                .Throttle(TimeSpan.FromSeconds(1))
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Subscribe(ep => UpdateTabIndexes()).DisposeWith(d);
        });
    }

    private void SetTitleBar()
    {
        AppWindow currentWindow = WindowsManager.GetWindowForElement(this)!;
        currentWindow.SetTitleBar(CustomDragRegion);
        CustomDragRegion.MinWidth = 188;
    }

    private void CloseTab(TabViewTabCloseRequestedEventArgs eventArgs)
    {
        if (eventArgs.Tab.Content is AppTab tab)
        {
            tabView.TabItems.Remove(eventArgs.Tab);

            if (tabView.TabItems.Count == 0)
            {
                Window? window = WindowsManager.GetWindowForElement(this);
                window?.Close();
            }

            tab.Dispose();
        }
    }

    private void NewTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        tabView.AddNewAppTab();
        args.Handled = true;
    }

    private void CloseSelectedTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        TabView tabView = (TabView)args.Element;
        TabViewItem tab = (TabViewItem)tabView.SelectedItem;
        if (tab is not null)
        {
            CloseSelectedTab(tabView, tab);
        }
        args.Handled = true;
    }

    private void CloseSelectedTab(TabView tabView, TabViewItem tab)
    {
        // Only remove the selected tab if it can be closed.
        if (tab.IsClosable == true)
        {
            tabView.TabItems.Remove(tab);
        }
    }

    private void NavigateToNumberedTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        TabView tabView = (TabView)args.Element;
        int tabToSelect = 0;

        switch (sender.Key)
        {
            case Windows.System.VirtualKey.Number1:
                tabToSelect = 0;
                break;
            case Windows.System.VirtualKey.Number2:
                tabToSelect = 1;
                break;
            case Windows.System.VirtualKey.Number3:
                tabToSelect = 2;
                break;
            case Windows.System.VirtualKey.Number4:
                tabToSelect = 3;
                break;
            case Windows.System.VirtualKey.Number5:
                tabToSelect = 4;
                break;
            case Windows.System.VirtualKey.Number6:
                tabToSelect = 5;
                break;
            case Windows.System.VirtualKey.Number7:
                tabToSelect = 6;
                break;
            case Windows.System.VirtualKey.Number8:
                tabToSelect = 7;
                break;
            case Windows.System.VirtualKey.Number9:
                // Select the last tab
                tabToSelect = tabView.TabItems.Count - 1;
                break;
        }

        // Only select the tab if it is in the list.
        if (tabToSelect < tabView.TabItems.Count)
        {
            tabView.SelectedIndex = tabToSelect;
        }
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

    public void Dispose()
    {
        disposables.Dispose();
    }

    private async Task RestoreOpenTabs()
    {
        BrowserTab[] tabs = await ViewModel!.GetOpenTabsAsync();

        foreach (BrowserTab item in tabs)
        {
            tabView.AddNewAppTab(item.Url, isSelected: item.IsTabSelected);
            Observable.FromAsync(async _ => await ViewModel!.DeleteTabAsync(item.Id)).Subscribe();
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
                        tabView.AddNewAppTab();
                        break;
                    case OnStartupSetting.RestoreOpenTabs:
                        await RestoreOpenTabs();
                        break;
                    case OnStartupSetting.OpenSpecificTabs:
                        break;
                    case OnStartupSetting.RestoreAndOpenNewTab:
                        break;
                    default:
                        tabView.AddNewAppTab();
                        break;
                }

                if (tabView.TabItems.Count == 0)
                {
                    tabView.AddNewAppTab();
                }

                tabView.SelectedItem ??= tabView.TabItems.First();

                UpdateTabIndexes();
                isLoaded = true;

            }).DisposeWith(disposables);
    }
}

public partial class ReactiveAppPage : ReactiveUserControl<AppPageViewModel>;