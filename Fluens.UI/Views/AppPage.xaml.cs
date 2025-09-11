using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels;
using Fluens.UI.Helpers;
using Fluens.UI.Wrappers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using ReactiveUI;
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

        ObservableTabView observableTabView = new(tabView);

        ViewModel = new AppPageViewModel(observableTabView);
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
