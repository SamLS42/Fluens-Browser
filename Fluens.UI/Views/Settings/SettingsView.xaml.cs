using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels.Settings;
using Fluens.UI.Helpers;
using Fluens.UI.Views.Settings.History;
using Microsoft.UI.Xaml.Controls;
using System.Reactive;
using System.Reactive.Linq;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Fluens.UI.Views.Settings;

public sealed partial class SettingsView : UserControl
{
    public SettingsViewModel ViewModel { get; set; }
    public SettingsView()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<SettingsViewModel>();

        NavView.MenuItems.AddRange(
            [
                new NavigationViewItem()
                {
                    Content = "Everything",
                    Tag = typeof(Everything),
                    Icon = new FontIcon() {Glyph = "\uF8B0" },
                },
                new NavigationViewItem()
                {
                    Content = "History",
                    Tag = typeof(HistoryPage),
                    Icon = new FontIcon() {Glyph = "\uE81C" },
                },
            ]);

        Observable.FromEventPattern<NavigationView, NavigationViewSelectionChangedEventArgs>(NavView, nameof(NavView.SelectionChanged))
            .Subscribe(NavigateToSelected);

        NavView.SelectedItem = NavView.MenuItems.First();
    }

    private void NavigateToSelected(EventPattern<NavigationView, NavigationViewSelectionChangedEventArgs> pattern)
    {
        DisposePrevious();

        NavFrame.BackStack.Clear();

        Type pageType = NavView.SelectedItem.As<NavigationViewItem>().Tag.As<Type>();

        switch (pageType)
        {
            case var t when t == typeof(HistoryPage):
                HistoryPage historyPage = NavFrame.Navigate<HistoryPage>();
                ViewModel.HistoryPageViewModel = historyPage.ViewModel!;
                break;
            case var t when t == typeof(Everything):
                NavFrame.Navigate<Everything>();
                break;
        }
    }

    private void DisposePrevious()
    {
        Type? previousPageType;

        if (NavFrame.BackStack.Count > 0)
        {
            previousPageType = NavFrame.BackStack.First().SourcePageType;

            switch (previousPageType)
            {
                case var t when t == typeof(HistoryPage):
                    ViewModel.HistoryPageViewModel?.Dispose();
                    ViewModel.HistoryPageViewModel = null;
                    break;
            }
        }
    }
}
