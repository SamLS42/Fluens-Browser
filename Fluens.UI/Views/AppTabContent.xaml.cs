using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels;
using Fluens.UI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.System;

namespace Fluens.UI.Views;

public sealed partial class AppTabContent : ReactiveAppTab, IDisposable
{
    private readonly CompositeDisposable Disposables = [];
    public AppTabContent()
    {
        InitializeComponent();

        this.WhenActivated(async d =>
        {
            await ActivateAsync();
        });

        this.WhenAnyValue(x => x.ViewModel)
            .WhereNotNull()
            .Subscribe(vm => vm.ReactiveWebView = new ReactiveWebView(WebView))
            .DisposeWith(Disposables);

        Observable.FromEventPattern<WebView2, CoreWebView2NavigationCompletedEventArgs>(WebView, nameof(WebView.NavigationCompleted))
                .Subscribe(ep => WebView.Focus(FocusState.Programmatic))
                .DisposeWith(Disposables);

        this.Bind(ViewModel, vm => vm.SearchBarText, v => v.SearchBar.Text).DisposeWith(Disposables);

        this.Bind(ViewModel, vm => vm.SettingsDialogIsOpen, v => v.SettingsPopup.IsOpen).DisposeWith(Disposables);
        this.OneWayBind(ViewModel, vm => vm.SettingsDialogIsOpen, v => v.ConfigBtn.IsChecked).DisposeWith(Disposables);
        this.OneWayBind(ViewModel, vm => vm.CanStop, v => v.StopBtn.Visibility).DisposeWith(Disposables);
        this.OneWayBind(ViewModel, vm => vm.CanRefresh, v => v.RefreshBtn.Visibility).DisposeWith(Disposables);
        this.OneWayBind(ViewModel, vm => vm.FaviconUrl, v => v.IconSource, IconSource.GetFromUrl).DisposeWith(Disposables);
        this.OneWayBind(ViewModel, vm => vm.DocumentTitle, v => v.Header, GetCorrectTitle).DisposeWith(Disposables);

        this.BindCommand(ViewModel, vm => vm.GoBack, v => v.GoBackBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.GoForward, v => v.GoForwardBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.Refresh, v => v.RefreshBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.Stop, v => v.StopBtn).DisposeWith(Disposables);
        this.BindCommand(ViewModel, vm => vm.ToggleSettingsDialogCommand, v => v.ConfigBtn).DisposeWith(Disposables);

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

        this.WhenAnyValue(x => x.ViewModel!.Url)
            .Where(url => url == Constants.AboutBlankUri)
            .Subscribe(_ => SearchBar.Focus(FocusState.Programmatic))
            .DisposeWith(Disposables);
    }

    private async Task ActivateAsync()
    {
        await WebView.EnsureCoreWebView2Async();
    }

    private void DetectEnterKey(VirtualKey key)
    {
        if (key == VirtualKey.Enter)
        {
            ViewModel?.NavigateToInputComman.Execute().Subscribe();
            WebView.Focus(FocusState.Programmatic);
        }
    }

    private static string GetCorrectTitle(string title)
    {
        return string.IsNullOrWhiteSpace(title)
                            || title.Equals(Constants.AboutBlankUri.ToString(), StringComparison.Ordinal)
                            ? Constants.NewTabTitle
                            : title;
    }

    public void Dispose()
    {
        Disposables.Dispose();
        ViewModel?.Dispose();
    }
}
public partial class ReactiveAppTab : TabViewItem, IViewFor<AppTabViewModel>, IActivatableView
{
    public AppTabViewModel? ViewModel { get; set; }
    object? IViewFor.ViewModel { get => ViewModel; set { ViewModel = (AppTabViewModel?)value; } }
}