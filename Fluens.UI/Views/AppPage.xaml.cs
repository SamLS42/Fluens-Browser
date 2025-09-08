using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels;
using Fluens.UI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ReactiveUI;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.System;

namespace Fluens.UI.Views;

public sealed partial class AppPage : ReactiveAppPage, IDisposable
{
    readonly CompositeDisposable disposables = [];
    public UIElement TitleBar => CustomDragRegion;

    public AppPage()
    {
        InitializeComponent();

        VerticalAlignment = VerticalAlignment.Stretch;

        ViewModel ??= ServiceLocator.GetRequiredService<AppPageViewModel>();

        this.OneWayBind(ViewModel, vm => vm.Tabs, v => v.tabView.TabItemsSource);

        this.Bind(ViewModel, vm => vm.SelectedItem, v => v.tabView.SelectedItem);

        Observable.FromEventPattern<TabView, object>(tabView, nameof(tabView.AddTabButtonClick))
            .Subscribe(async _ =>
            {
                await ViewModel!.CreateNewTabAsync();
            });

        Observable.FromEventPattern<TabView, TabViewTabCloseRequestedEventArgs>(tabView, nameof(tabView.TabCloseRequested))
            .Subscribe(async pattern => await ViewModel.CloseTabAsync((AppTabViewModel)pattern.EventArgs.Item));

        Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(ViewModel.Tabs, nameof(ViewModel.Tabs.CollectionChanged))
            .Where(ep => ep.EventArgs.Action != NotifyCollectionChangedAction.Move)
            .SelectMany(_ => ViewModel.Tabs.Select(vm => vm.KeyboardShortcuts))
            .Switch()
            .Subscribe(async s => await ViewModel.HandleKeyboardShortcutAsync(s))
            .DisposeWith(disposables);
    }

    public void Dispose()
    {
        disposables.Dispose();
        ViewModel?.Dispose();
    }

    private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ShortcutMessage shortcutMessage = new()
        {
            Key = args.KeyboardAccelerator.Key.ToString().ToUpperInvariant(),
            Ctrl = args.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Control),
            Shift = args.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Shift),
        };

        Observable.FromAsync(_ => ViewModel!.HandleKeyboardShortcutAsync(shortcutMessage)).Subscribe();

        args.Handled = true;
    }
}

public partial class ReactiveAppPage : ReactiveUserControl<AppPageViewModel>;
