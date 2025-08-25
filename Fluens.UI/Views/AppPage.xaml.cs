using Fluens.AppCore.Enums;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels;
using Fluens.UI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.Foundation.Collections;
using WinRT;

namespace Fluens.UI.Views;

public sealed partial class AppPage : ReactiveAppPage, IDisposable
{
    private readonly CompositeDisposable disposables = [];
    public UIElement TitleBar => CustomDragRegion;

    private readonly Subject<Unit> hasNoTabs = new();
    public IObservable<Unit> HasNoTabs => hasNoTabs.AsObservable();

    private readonly ObservableCollection<TabViewItem> tabs = [];

    public AppPage()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<AppPageViewModel>();

        Observable.FromEventPattern<TabView, object>(tabView, nameof(tabView.AddTabButtonClick))
            .Subscribe(async _ => await AddBlankTabAsync());

        Observable.FromEventPattern<SelectionChangedEventArgs>(tabView, nameof(tabView.SelectionChanged))
            .Subscribe(ep =>
            {
                ep.EventArgs.RemovedItems.FirstOrDefault()?.As<TabViewItem>().ViewModel!.IsSelected = false;
                ep.EventArgs.AddedItems.FirstOrDefault()?.As<TabViewItem>().ViewModel!.IsSelected = true;
            });

        Observable.FromEventPattern<TabView, TabViewTabCloseRequestedEventArgs>(tabView, nameof(tabView.TabCloseRequested))
            .Subscribe(async pattern => await RemoveTabAsync(pattern));

        Observable.FromEventPattern<TabView, IVectorChangedEventArgs>(tabView, nameof(tabView.TabItemsChanged))
            .Subscribe(ep => UpdateTabIndexes());
    }

    private async Task AddBlankTabAsync()
    {
        int id = await ViewModel!.GetNewTabId();
        AppTabViewModel vm = new(id, Constants.AboutBlankUri);
        AddTabViewItem(vm);
    }

    private void AddTabViewItem(AppTabViewModel vm)
    {
        TabViewItem tabViewItem = CreateTabItem(vm);
        tabs.Add(tabViewItem);
        if (vm.IsSelected)
        {
            tabView.SelectedItem = tabViewItem;
        }
    }

    private TabViewItem CreateTabItem(AppTabViewModel vm)
    {
        AppTab appTab = new(vm);

        TabViewItem newTab = new()
        {
            Header = UIConstants.NewTabTitle,
            IconSource = UIConstants.BlankPageIcon,
            Content = appTab
        };

        vm.FaviconUrl.Subscribe(faviconUrl => newTab.IconSource = IconSource.GetFromUrl(faviconUrl));
        vm.DocumentTitle.Subscribe(title => newTab.Header = GetCorrectTitle(title));

        return newTab;
    }

    private static string GetCorrectTitle(string? title)
    {
        return string.IsNullOrWhiteSpace(title) || title.Equals(Constants.AboutBlankUri.ToString(), StringComparison.Ordinal)
            ? UIConstants.NewTabTitle
            : title;
    }

    private void UpdateTabIndexes()
    {
        foreach (TabViewItem tabItem in tabs.OfType<TabViewItem>())
        {
            AppTab appTab = tabItem.Content.As<AppTab>();
            int newIndex = tabs.IndexOf(tabItem);
            appTab.ViewModel?.Index = newIndex;
        }
    }

    private async Task RemoveTabAsync(EventPattern<TabView, TabViewTabCloseRequestedEventArgs> pattern)
    {
        AppTab tab = pattern.EventArgs.Tab.Content.As<AppTab>();
        tabs.Remove(pattern.EventArgs.Tab);
        await ViewModel!.DeleteTabAsync(tab.ViewModel!);
        tab.Dispose();
    }

    public async Task ApplyOnStartupSettingAsync(OnStartupSetting onStartupSetting)
    {
        switch (onStartupSetting)
        {
            case OnStartupSetting.OpenNewTab:
                await AddBlankTabAsync();
                break;
            case OnStartupSetting.RestoreOpenTabs:
                foreach (AppTabViewModel vm in await ViewModel!.GetOpenTabsAsync())
                {
                    AddTabViewItem(vm);
                }
                break;
            //TODO
            //case OnStartupSetting.OpenSpecificTabs:
            //    break;
            case OnStartupSetting.RestoreAndOpenNewTab:
                foreach (AppTabViewModel vm in await ViewModel!.GetOpenTabsAsync())
                {
                    AddTabViewItem(vm);
                }
                await AddBlankTabAsync();
                break;
            default:
                await AddBlankTabAsync();
                break;
        }

        if (tabs.Count == 0)
        {
            await AddBlankTabAsync();
        }
    }

    public void Dispose()
    {
        disposables.Dispose();
        hasNoTabs.OnCompleted();
        hasNoTabs.Dispose();
    }
}

public partial class ReactiveAppPage : ReactiveUserControl<AppPageViewModel>;