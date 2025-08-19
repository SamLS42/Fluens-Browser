using DynamicData;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArgyllBrowse.UI.Views.Settings;
public sealed partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();

        NavView.MenuItems.AddRange(
            [
                new NavigationViewItem()
                {
                    Content = "Everything",
                    Tag = typeof(Everything),
                    Icon = new FontIcon() {Glyph = "\uF8B0" },
                },
            ]);

        Observable.FromEventPattern<NavigationView, NavigationViewSelectionChangedEventArgs>(NavView, nameof(NavView.SelectionChanged))
            .Subscribe(_ => NavigateToSelected());

        NavView.SelectedItem = NavView.MenuItems.First();
    }

    private void NavigateToSelected()
    {
        Type pageType = NavView.SelectedItem.As<NavigationViewItem>().Tag.As<Type>();

        NavFrame.Navigate(pageType, null, new CommonNavigationTransitionInfo());
    }
}
