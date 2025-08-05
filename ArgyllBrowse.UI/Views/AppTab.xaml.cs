using ArgyllBrowse.UI.Helpers;
using ArgyllBrowse.UI.ViewModels;
using ArgyllBrowse.UI.ViewModels.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.System;

namespace ArgyllBrowse.UI.Views;
public partial class ReactiveAppTab : ReactiveUserControl<AppTabViewModel>;
public sealed partial class AppTab : ReactiveAppTab, IDisposable
{
    private readonly CompositeDisposable Disposables = [];

    public AppTab()
    {
        InitializeComponent();

        MyWebView.Source = Constants.AboutBlankUri;

        ViewModel ??= ServiceLocator.GetRequiredService<AppTabViewModel>();
        ViewModel.SetReactiveWebView(new ReactiveWebView(MyWebView));

        this.Bind(ViewModel, vm => vm.SearchBarText, v => v.SearchBar.Text).DisposeWith(Disposables);
        this.OneWayBind(ViewModel, vm => vm.CanStop, v => v.StopBtn.Visibility).DisposeWith(Disposables);
        this.OneWayBind(ViewModel, vm => vm.CanRefresh, v => v.RefreshBtn.Visibility).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.GoBack, v => v.GoBackBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.GoForward, v => v.GoForwardBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.Refresh, v => v.RefreshBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.Stop, v => v.StopBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.OpenConfig, v => v.ConfigBtn).DisposeWith(Disposables);

        Observable.FromEventPattern<KeyRoutedEventArgs>(SearchBar, nameof(SearchBar.KeyDown))
            .Subscribe(ep => DetectEnterKey(ep.EventArgs.Key));

        Observable.FromEventPattern<RoutedEventArgs>(SearchBar, nameof(SearchBar.GotFocus))
            .Subscribe(_ => SearchBar.SelectAll());


        this.WhenActivated(async d =>
        {
            if (MyWebView.Source == Constants.AboutBlankUri && !string.IsNullOrWhiteSpace(SearchBar.Text))
            {
                await MyWebView.EnsureCoreWebView2Async();
                ViewModel?.NavigateToSeachBarInput.Execute().Subscribe();
            }
            else if (MyWebView.Source == Constants.AboutBlankUri)
            {
                SearchBar.Focus(FocusState.Programmatic);
            }
        });
    }

    private void DetectEnterKey(VirtualKey key)
    {
        if (key == VirtualKey.Enter)
        {
            ViewModel?.NavigateToSeachBarInput.Execute().Subscribe();
        }
    }

    private void OpenDevToolsWindow_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        Observable.FromAsync(() => MyWebView.EnsureCoreWebView2Async().AsTask())
            .Subscribe(_ => MyWebView.CoreWebView2.OpenDevToolsWindow());
    }

    public void Dispose()
    {
        Disposables.Dispose();
        ViewModel?.Dispose();
    }
}
