using ArgyllBrowse.AppCore.Enums;
using ArgyllBrowse.AppCore.Helpers;
using ArgyllBrowse.AppCore.ViewModels;
using ArgyllBrowse.Data.Entities;
using ArgyllBrowse.UI.Helpers;
using ArgyllBrowse.UI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ReactiveUI;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using Windows.Foundation.Collections;
using WinRT;

namespace ArgyllBrowse.UI.Views;

public sealed partial class AppPage : ReactiveAppPage
{
    private WindowsManager WindowsManager { get; } = ServiceLocator.GetRequiredService<WindowsManager>();
    private static Window? TabTearOutWindow { get; set; }
    public UIElement TitleBar => CustomDragRegion;

    public AppPage()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<AppPageViewModel>();

        Observable.FromEventPattern<TabView, object>(tabView, nameof(tabView.AddTabButtonClick))
            .Subscribe(_ => tabView.AddEmptyTab());

        Observable.FromEventPattern<SelectionChangedEventArgs>(tabView, nameof(tabView.SelectionChanged))
            .Subscribe(ep =>
            {
                ep.EventArgs.RemovedItems.FirstOrDefault()?.As<TabViewItem>().Content.As<AppTab>().ViewModel?.IsTabSelected = false;
                ep.EventArgs.AddedItems.FirstOrDefault()?.As<TabViewItem>().Content.As<AppTab>().ViewModel?.IsTabSelected = true;
            });

        Observable.FromEventPattern<TabView, TabViewTabCloseRequestedEventArgs>(tabView, nameof(tabView.TabCloseRequested))
            .Subscribe(ep => RemoveTab(ep.EventArgs.Tab));
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

    private void UpdateTabIndexes(EventPattern<TabView, IVectorChangedEventArgs> pattern)
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

                Observable.FromEventPattern<TabView, IVectorChangedEventArgs>(tabView, nameof(tabView.TabItemsChanged))
                    .Throttle(TimeSpan.FromMilliseconds(200), RxApp.MainThreadScheduler)
                    .Subscribe(UpdateTabIndexes);
            });
    }

    private class AppTabViewItemComparer : IEqualityComparer<TabViewItem>
    {
        private static readonly Lazy<AppTabViewItemComparer> lazy = new(() => new AppTabViewItemComparer());

        public static AppTabViewItemComparer Instance => lazy.Value;

        public bool Equals(TabViewItem? x, TabViewItem? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Content.As<AppTab>().ViewModel!.TabId == y.Content.As<AppTab>().ViewModel!.TabId;
        }

        public int GetHashCode([DisallowNull] TabViewItem obj)
        {
            return obj.Content.As<AppTab>().ViewModel!.TabId.GetHashCode();
        }
    }
}

public partial class ReactiveAppPage : ReactiveUserControl<AppPageViewModel>;