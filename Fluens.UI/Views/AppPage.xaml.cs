using DynamicData;
using Fluens.AppCore.Contracts;
using Fluens.AppCore.Enums;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels;
using Fluens.UI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.Foundation.Collections;
using Windows.System;
using WinRT;

namespace Fluens.UI.Views;

public sealed partial class AppPage : ReactiveAppPage, IDisposable, ITabView
{
    readonly CompositeDisposable disposables = [];
    public UIElement TitleBar => CustomDragRegion;

    private readonly Subject<Unit> hasNoTabs = new();
    public IObservable<Unit> HasNoTabs => hasNoTabs.AsObservable();

    private readonly ObservableCollection<TabViewItem> tabs = []; //For some reason, adding tabs is faster (visually) when using TabItemsSource instead of using Items directly
    private readonly SourceCache<TabViewItem, int> tabsSource = new(tvi => tvi.ViewModel.Id);


    public AppPage()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<AppPageViewModel>();

        Observable.FromEventPattern<TabView, object>(tabView, nameof(tabView.AddTabButtonClick))
            .Subscribe(async _ => await AddTabAsync());

        Observable.FromEventPattern<SelectionChangedEventArgs>(tabView, nameof(tabView.SelectionChanged))
            .Subscribe(ep =>
            {
                ep.EventArgs.RemovedItems.FirstOrDefault()?.As<TabViewItem>().ViewModel!.IsSelected = false;
                ep.EventArgs.AddedItems.FirstOrDefault()?.As<TabViewItem>().ViewModel!.IsSelected = true;
            });

        Observable.FromEventPattern<TabView, TabViewTabCloseRequestedEventArgs>(tabView, nameof(tabView.TabCloseRequested))
            .Subscribe(async pattern => await CloseTabAsync(pattern.EventArgs.Tab));

        Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(tabs, nameof(tabs.CollectionChanged))
            .Subscribe(_ =>
            {
                if (tabs.Count == 0)
                {
                    hasNoTabs.OnNext(Unit.Default);
                }

                tabsSource.EditDiff(tabs, TabViewItemComparer.Instance);
            });

        Observable.FromEventPattern<TabView, IVectorChangedEventArgs>(tabView, nameof(tabView.TabItemsChanged))
            .Subscribe(ep => UpdateTabIndexes());

        tabsSource.Connect()
            .MergeMany(tabView => tabView.ViewModel.KeyboardShortcuts)
            .Subscribe(async s => await HandleKeyboardShortcutAsync(s))
            .DisposeWith(disposables);
    }

    private async Task HandleKeyboardShortcutAsync(ShortcutMessage message)
    {
        switch (message)
        {
            case { Ctrl: true, Key: "T" }:
                await AddTabAsync();
                break;
            case { Ctrl: true, Key: "W" }:
                await CloseTabAsync(tabView.SelectedItem.As<TabViewItem>());
                break;
        }
    }

    public async Task AddTabAsync(Uri? uri = null, bool isSelected = true, bool activate = false)
    {
        AppTabViewModel vm = await ViewModel!.CreateTabAsync(uri, isSelected);
        await AddTabViewItemAsync(vm, activate);
    }

    private async Task AddTabViewItemAsync(AppTabViewModel vm, bool activate = false)
    {
        TabViewItem tabViewItem = await CreateTabItemAsync(vm, activate);
        tabs.Add(tabViewItem);
        if (vm.IsSelected)
        {
            tabView.SelectedItem = tabViewItem;
        }
    }

    private async Task<TabViewItem> CreateTabItemAsync(AppTabViewModel vm, bool activate = false)
    {
        AppTab appTab = new(vm);

        TabViewItem newTab = new()
        {
            Header = UIConstants.NewTabTitle,
            IconSource = UIConstants.BlankPageIcon,
            Content = appTab
        };

        vm.FaviconUrl.Subscribe(faviconUrl => newTab.IconSource = IconSource.GetFromUrl(faviconUrl));
        vm.DocumentTitle.Subscribe(title =>
        {
            newTab.Header = string.IsNullOrWhiteSpace(title)
                || title.Equals(Constants.AboutBlankUri.ToString(), StringComparison.Ordinal)
                ? UIConstants.NewTabTitle
                : title;
        });

        if (activate)
        {
            await appTab.ActivateAsync();
        }

        return newTab;
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

    private async Task CloseTabAsync(TabViewItem tabView)
    {
        AppTab tab = tabView.AppTab;
        tabs.Remove(tabView);
        await ViewModel!.DeleteTabAsync(tab.ViewModel!);
        tab.Dispose();
    }

    private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ShortcutMessage shortcutMessage = new()
        {
            Key = args.KeyboardAccelerator.Key.ToString().ToUpperInvariant(),
            Ctrl = args.KeyboardAccelerator.Modifiers is VirtualKeyModifiers.Control,
            Shift = args.KeyboardAccelerator.Modifiers is VirtualKeyModifiers.Shift,
        };

        Observable.FromAsync(_ => HandleKeyboardShortcutAsync(shortcutMessage)).Subscribe();
    }

    public async Task ApplyOnStartupSettingAsync(OnStartupSetting onStartupSetting)
    {
        switch (onStartupSetting)
        {
            case OnStartupSetting.OpenNewTab:
                await AddTabAsync();
                break;
            case OnStartupSetting.RestoreOpenTabs:
                foreach (AppTabViewModel vm in await ViewModel!.RecoverTabsAsync())
                {
                    await AddTabViewItemAsync(vm);
                }
                break;
            //TODO
            //case OnStartupSetting.OpenSpecificTabs:
            //    break;
            case OnStartupSetting.RestoreAndOpenNewTab:
                foreach (AppTabViewModel vm in await ViewModel!.RecoverTabsAsync())
                {
                    await AddTabViewItemAsync(vm);
                }
                await AddTabAsync();
                break;
            default:
                await AddTabAsync();
                break;
        }

        if (tabs.Count == 0)
        {
            await AddTabAsync();
        }
    }

    public bool HasTab(AppTabViewModel tab)
    {
        return tabs.Any(t => t.ViewModel == tab);
    }

    public void Dispose()
    {
        tabsSource.Dispose();
        disposables.Dispose();
        hasNoTabs.OnCompleted();
        hasNoTabs.Dispose();
    }
}

public partial class ReactiveAppPage : ReactiveUserControl<AppPageViewModel>;