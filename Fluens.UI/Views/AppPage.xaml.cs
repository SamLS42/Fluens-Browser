using DynamicData;
using Fluens.AppCore.Contracts;
using Fluens.AppCore.Enums;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels;
using Fluens.UI.Helpers;
using Fluens.UI.Services;
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

public sealed partial class AppPage : ReactiveAppPage, IDisposable, ITabPage
{
    readonly CompositeDisposable disposables = [];
    public UIElement TitleBar => CustomDragRegion;

    private readonly Subject<Unit> hasNoTabs = new();
    public IObservable<Unit> HasNoTabs => hasNoTabs.AsObservable();

    private readonly ObservableCollection<AppTabViewItem> tabs = []; //For some reason, adding tabs is faster (visually) when using TabItemsSource instead of using Items directly
    private readonly SourceCache<AppTabViewItem, int> tabsSource = new(tvi => tvi.ViewModel!.Id);

    private WindowsManager WindowsManager { get; } = ServiceLocator.GetRequiredService<WindowsManager>();

    public AppPage()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<AppPageViewModel>();

        Observable.FromEventPattern<TabView, object>(tabView, nameof(tabView.AddTabButtonClick))
            .Subscribe(async _ =>
            {
                tabView.SelectedItem = CreateTabViewItem(await ViewModel!.CreateTabAsync());
            });

        Observable.FromEventPattern<SelectionChangedEventArgs>(tabView, nameof(tabView.SelectionChanged))
            .Subscribe(ep =>
            {
                ep.EventArgs.RemovedItems.FirstOrDefault()?.As<AppTabViewItem>().ViewModel!.IsSelected = false;

                if (ep.EventArgs.AddedItems.FirstOrDefault()?.As<AppTabViewItem>() is AppTabViewItem appTab)
                {
                    appTab.ViewModel!.IsSelected = true;
                }
            });

        Observable.FromEventPattern<TabView, TabViewTabCloseRequestedEventArgs>(tabView, nameof(tabView.TabCloseRequested))
            .Subscribe(async pattern => await CloseTabAsync(pattern.EventArgs.Tab.As<AppTabViewItem>()));

        Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(tabs, nameof(tabs.CollectionChanged))
            .Subscribe(_ =>
            {
                if (tabs.Count == 0)
                {
                    hasNoTabs.OnNext(Unit.Default);
                }

                tabsSource.EditDiff(tabs, areItemsEqual: (i1, i2) => i1.ViewModel!.Id == i2.ViewModel!.Id);
            });

        Observable.FromEventPattern<TabView, IVectorChangedEventArgs>(tabView, nameof(tabView.TabItemsChanged))
            .Subscribe(ep => UpdateTabIndexes());

        tabsSource.Connect()
            .MergeMany(tabView => tabView.ViewModel!.KeyboardShortcuts)
            .Subscribe(async s => await HandleKeyboardShortcutAsync(s))
            .DisposeWith(disposables);
    }

    private async Task HandleKeyboardShortcutAsync(ShortcutMessage message)
    {
        switch (message)
        {
            case { Ctrl: true, Shift: true, Key: "T" }:
                await RestoreClosedTabAsync();
                break;
            case { Ctrl: true, Key: "T" }:
                tabView.SelectedItem = CreateTabViewItem(await ViewModel!.CreateTabAsync());
                break;
            case { Ctrl: true, Key: "W" }:
                await CloseTabAsync(tabView.SelectedItem.As<AppTabViewItem>());
                break;
            case { Key: "F5" }:
                tabView.SelectedItem.As<AppTabViewItem>().ViewModel!.Refresh.Execute().Subscribe();
                break;
        }
    }

    private async Task RestoreClosedTabAsync()
    {
        AppTabViewModel? vm = await ViewModel!.GetClosedTabAsync();

        if (vm is null)
        {
            return;
        }

        tabView.SelectedItem = CreateTabViewItem(vm);
    }

    public IViewFor<AppTabViewModel> CreateTabViewItem(AppTabViewModel vm)
    {
        AppTabViewItem tabViewItem = CreateTabItemAsync(vm);

        if (vm.Index != null)
        {
            tabs.Insert(vm.Index.Value, tabViewItem);
        }
        else
        {
            tabs.Add(tabViewItem);
        }

        return tabViewItem;
    }

    private AppTabViewItem CreateTabItemAsync(AppTabViewModel vm)
    {
        AppTabViewItem appTab = new()
        {
            ViewModel = vm,
            Header = Constants.NewTabTitle,
            IconSource = UIConstants.BlankPageIcon,
        };

        return appTab;
    }

    private void UpdateTabIndexes()
    {
        foreach (AppTabViewItem tabItem in tabs)
        {
            tabItem.ViewModel?.Index = tabs.IndexOf(tabItem);
        }
    }

    private async Task CloseTabAsync(AppTabViewItem tabView)
    {
        tabs.Remove(tabView);
        await ViewModel!.CloseTabAsync(tabView.ViewModel!.Id);
        tabView.Dispose();
    }

    public async Task ApplyOnStartupSettingAsync(OnStartupSetting onStartupSetting)
    {
        ViewModel!.WindowId = WindowsManager.GetParentWindowId(this);

        switch (onStartupSetting)
        {
            case OnStartupSetting.OpenNewTab:
                tabView.SelectedItem = CreateTabViewItem(await ViewModel!.CreateTabAsync());
                break;
            case OnStartupSetting.RestoreOpenTabs:
                foreach (AppTabViewModel vm in await ViewModel!.RecoverTabsAsync())
                {
                    CreateTabViewItem(vm);
                }
                break;
            //TODO
            //case OnStartupSetting.OpenSpecificTabs:
            //    break;
            case OnStartupSetting.RestoreAndOpenNewTab:
                foreach (AppTabViewModel vm in await ViewModel!.RecoverTabsAsync())
                {
                    CreateTabViewItem(vm);
                }
                tabView.SelectedItem = CreateTabViewItem(await ViewModel!.CreateTabAsync());
                break;
            default:
                tabView.SelectedItem = CreateTabViewItem(await ViewModel!.CreateTabAsync());
                break;
        }

        if (tabs.Count == 0)
        {
            tabView.SelectedItem = CreateTabViewItem(await ViewModel!.CreateTabAsync());
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

    private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ShortcutMessage shortcutMessage = new()
        {
            Key = args.KeyboardAccelerator.Key.ToString().ToUpperInvariant(),
            Ctrl = args.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Control),
            Shift = args.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Shift),
        };

        Observable.FromAsync(_ => HandleKeyboardShortcutAsync(shortcutMessage)).Subscribe();

        args.Handled = true;
    }
}

public partial class ReactiveAppPage : ReactiveUserControl<AppPageViewModel>;