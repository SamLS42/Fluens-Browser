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
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.Foundation.Collections;
using Windows.System;
using WinRT;

namespace Fluens.UI.Views;

public sealed partial class AppPage : ReactiveAppPage, IDisposable, ITabPage
{
    readonly CompositeDisposable disposables = [];
    public UIElement TitleBar => CustomDragRegion;

    private WindowsManager WindowsManager { get; } = ServiceLocator.GetRequiredService<WindowsManager>();

    public AppPage()
    {
        InitializeComponent();

        VerticalAlignment = VerticalAlignment.Stretch;

        ViewModel ??= ServiceLocator.GetRequiredService<AppPageViewModel>();

        this.OneWayBind(ViewModel, vm => vm.TabsSource, v => v.tabView.TabItemsSource);

        Observable.FromEventPattern<TabView, object>(tabView, nameof(tabView.AddTabButtonClick))
            .Subscribe(async _ =>
            {
                AppTabViewModel vm = await ViewModel!.CreateTabAsync();
                CreateTabViewItem(vm);
                tabView.SelectedItem = vm;
            });

        Observable.FromEventPattern<SelectionChangedEventArgs>(tabView, nameof(tabView.SelectionChanged))
            .Subscribe(ep =>
            {
                ep.EventArgs.RemovedItems.OfType<AppTabViewModel>().FirstOrDefault()?.IsSelected = false;

                if (ep.EventArgs.AddedItems.OfType<AppTabViewModel>().FirstOrDefault() is AppTabViewModel vm)
                {
                    vm.IsSelected = true;
                }
            });

        Observable.FromEventPattern<TabView, TabViewTabCloseRequestedEventArgs>(tabView, nameof(tabView.TabCloseRequested))
            .Subscribe(async pattern => await CloseTabAsync((AppTabViewModel)pattern.EventArgs.Item));

        Observable.FromEventPattern<TabView, IVectorChangedEventArgs>(tabView, nameof(tabView.TabItemsChanged))
            .Subscribe(ep => UpdateTabIndexes());

        Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(ViewModel.TabsSource, nameof(ViewModel.TabsSource.CollectionChanged))
            .Where(ep => ep.EventArgs.Action != NotifyCollectionChangedAction.Move)
            .SelectMany(_ => ViewModel.TabsSource.Select(vm => vm.KeyboardShortcuts))
            .Switch()
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
                await CreateNewTab();
                break;
            case { Ctrl: true, Key: "W" }:
                await CloseTabAsync((AppTabViewModel)tabView.SelectedItem);
                break;
            case { Key: "F5" }:
                tabView.SelectedItem.As<AppTabContent>().ViewModel!.Refresh.Execute().Subscribe();
                break;
        }
    }

    private async Task CreateNewTab()
    {
        AppTabViewModel vm = await ViewModel!.CreateTabAsync();
        CreateTabViewItem(vm);
        tabView.SelectedItem = vm;
    }

    private async Task RestoreClosedTabAsync()
    {
        AppTabViewModel? vm = await ViewModel!.GetClosedTabAsync();

        if (vm is null)
        {
            return;
        }

        tabView.SelectedItem = vm;
    }

    public void CreateTabViewItem(AppTabViewModel vm)
    {
        if (vm.Index != null)
        {
            ViewModel!.TabsSource.Insert(vm.Index.Value, vm);
        }
        else
        {
            ViewModel!.TabsSource.Add(vm);
        }
    }

    private void UpdateTabIndexes()
    {
        foreach (AppTabViewModel vm in ViewModel!.TabsSource)
        {
            vm.Index = ViewModel!.TabsSource.IndexOf(vm);
        }
    }

    private async Task CloseTabAsync(AppTabViewModel vm)
    {
        ViewModel!.TabsSource.Remove(vm);
        await ViewModel!.CloseTabAsync(vm.Id);
        vm.Dispose();
    }

    public async Task ApplyOnStartupSettingAsync(OnStartupSetting onStartupSetting)
    {
        ViewModel!.WindowId = WindowsManager.GetParentWindowId(this);

        switch (onStartupSetting)
        {
            case OnStartupSetting.OpenNewTab:
                await CreateNewTab();
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
                await CreateNewTab();
                break;
            default:
                await CreateNewTab();
                break;
        }

        if (ViewModel.TabsSource.Count == 0)
        {
            await CreateNewTab();
        }
    }

    public bool HasTab(AppTabViewModel tab)
    {
        return ViewModel!.TabsSource.Any(t => t == tab);
    }

    public void Dispose()
    {
        disposables.Dispose();
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
