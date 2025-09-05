using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels;
using Fluens.UI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.System;

namespace Fluens.UI.Views;

public partial class ReactiveAppTab : ReactiveUserControl<AppTabViewModel>;
public sealed partial class AppTab : ReactiveAppTab, IDisposable
{
    private readonly CompositeDisposable Disposables = [];
    private readonly ReactiveWebView reactiveWebView;
    public AppTab(AppTabViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;

        reactiveWebView = new() { MyWebView = WebView };
        ViewModel!.SetReactiveWebView(reactiveWebView);

        ViewModel.NavigationCompleted
            .Subscribe(_ => WebView.Focus(FocusState.Programmatic))
            .DisposeWith(Disposables);

        this.Bind(ViewModel, vm => vm.SearchBarText, v => v.SearchBar.Text).DisposeWith(Disposables);

        this.Bind(ViewModel, vm => vm.SettingsDialogIsOpen, v => v.SettingsPopup.IsOpen).DisposeWith(Disposables);
        this.OneWayBind(ViewModel, vm => vm.SettingsDialogIsOpen, v => v.ConfigBtn.IsChecked).DisposeWith(Disposables);
        this.OneWayBind(ViewModel, vm => vm.CanStop, v => v.StopBtn.Visibility).DisposeWith(Disposables);
        this.OneWayBind(ViewModel, vm => vm.CanRefresh, v => v.RefreshBtn.Visibility).DisposeWith(Disposables);

        this.BindCommand(ViewModel, vm => vm.GoBack, v => v.GoBackBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.GoForward, v => v.GoForwardBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.Refresh, v => v.RefreshBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.Stop, v => v.StopBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.ToggleSettingsDialogIsOpen, v => v.ConfigBtn).DisposeWith(Disposables);

        Observable.FromEventPattern<KeyRoutedEventArgs>(SearchBar, nameof(SearchBar.KeyDown))
            .Subscribe(ep => DetectEnterKey(ep.EventArgs.Key));

        Observable.FromEventPattern<RoutedEventArgs>(SearchBar, nameof(SearchBar.GotFocus))
            .Subscribe(_ => SearchBar.SelectAll());

        Observable.FromEventPattern<SizeChangedEventArgs>(WebView, nameof(WebView.SizeChanged))
            .Subscribe(ep =>
            {
                SettingsDialogContent.Width = WebView.ActualWidth - (2 * SettingsDialogContent.Margin.Left);
                SettingsDialogContent.Height = WebView.ActualHeight - (2 * SettingsDialogContent.Margin.Top);
            });

        this.WhenActivated(async d =>
        {
            await ActivateAsync();
        });

        this.WhenAnyValue(x => x.SettingsView.ViewModel.HistoryPageViewModel)
            .WhereNotNull()
            .Select(vm => vm.Entries.Connect()
                .MergeMany(entry => entry.OpenUrl.Select(_ => entry.Url))
            )
            .Switch()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(url =>
            {
                WebView.Source = url;
                ViewModel!.SettingsDialogIsOpen = false;
            })
            .DisposeWith(Disposables);
    }

    public async Task ActivateAsync()
    {
        WebView.Focus(FocusState.Programmatic);

        if (WebView.Source is null &&
            !string.IsNullOrWhiteSpace(SearchBar.Text) &&
            Constants.AboutBlankUri.ToString() != SearchBar.Text)
        {
            ViewModel?.NavigateToSearchBarInput.Execute().Subscribe();
        }
        else if (ViewModel!.Url == Constants.AboutBlankUri)
        {
            SearchBar.Focus(FocusState.Programmatic);
        }

        await Task.CompletedTask;
    }

    private void DetectEnterKey(VirtualKey key)
    {
        if (key == VirtualKey.Enter)
        {
            ViewModel?.NavigateToSearchBarInput.Execute().Subscribe();
            WebView.Focus(FocusState.Programmatic);
        }
    }

    public void Dispose()
    {
        Disposables.Dispose();
        reactiveWebView.Dispose();
        ViewModel?.Dispose();
    }
}
