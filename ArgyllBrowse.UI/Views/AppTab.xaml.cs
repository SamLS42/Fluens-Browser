using ArgyllBrowse.UI.Helpers;
using ArgyllBrowse.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WebUI;

namespace ArgyllBrowse.UI.Views;
public partial class ReactiveAppTab : ReactiveUserControl<AppTabViewModel>;
public sealed partial class AppTab : ReactiveAppTab, IDisposable
{
    private readonly CompositeDisposable Disposables = [];

    public AppTab()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<AppTabViewModel>();
        ViewModel.SetReactiveWebView(new ReactiveWebView(MyWebView));

        this.WhenActivated(d =>
        {
            this.Bind(ViewModel, vm => vm.SearchBarText, v => v.SearchBar.Text).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CanStop, v => v.StopBtn.Visibility).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CanRefresh, v => v.RefreshBtn.Visibility).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.GoBack, v => v.GoBackBtn).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.GoForward, v => v.GoForwardBtn).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.Refresh, v => v.RefreshBtn).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.Stop, v => v.StopBtn).DisposeWith(d);

            Observable.FromEventPattern<KeyRoutedEventArgs>(SearchBar, nameof(SearchBar.KeyDown))
                .Subscribe(ep => DetectEnterKey(ep.EventArgs)).DisposeWith(d);
        });

        this.Bind(ViewModel, vm => vm.Url, v => v.MyWebView.Source).DisposeWith(Disposables);
    }

    private void DetectEnterKey(KeyRoutedEventArgs eventArgs)
    {
        if (eventArgs.Key == Windows.System.VirtualKey.Enter)
        {
            ViewModel?.NavigateToUrl.Execute().Subscribe();
        }
    }

    public void Dispose()
    {
        Disposables.Dispose();
        ViewModel?.Dispose();
    }

    private void OpenDevToolsWindow_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        Observable.FromAsync(() => MyWebView.EnsureCoreWebView2Async().AsTask())
            .Subscribe(_ => MyWebView.CoreWebView2.OpenDevToolsWindow());
    }
}
