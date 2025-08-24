using ArgyllBrowse.AppCore.Helpers;
using ArgyllBrowse.Data.Entities;
using ArgyllBrowse.UI.Views;
using Microsoft.UI.Xaml.Controls;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ArgyllBrowse.UI.Helpers;
internal static class TabViewExtensions
{
    private const string newTabTitle = "New Tab";
    private static readonly FontIconSource loadingPageIcon = new() { Glyph = "\uF16A" };

    internal static void AddEmptyTab(this TabView tabView)
    {
        ArgumentNullException.ThrowIfNull(tabView);

        AppTab appTab = new();
        CompositeDisposable appTabDisposable = [];

        TabViewItem newTab = new()
        {
            Header = newTabTitle,
            IconSource = UIConstants.BlankPageIcon,
            Content = appTab
        };

        tabView.TabItems.Add(newTab);

        tabView.SelectedItem = newTab;

        SubscribeToTabChanges(appTab, appTabDisposable, newTab);
    }

    internal static void AddAppTab(this TabView tabView, BrowserTab item)
    {
        ArgumentNullException.ThrowIfNull(tabView);

        AppTab appTab = new(item.Id, item.Index, item.DocumentTitle, item.FaviconUrl, new Uri(item.Url));

        CompositeDisposable appTabDisposable = [];

        TabViewItem newTab = new()
        {
            Content = appTab
        };

        tabView.TabItems.Add(newTab);

        SubscribeToTabChanges(appTab, appTabDisposable, newTab);

        if (item is not null)
        {
            if (item.IsTabSelected)
            {
                tabView.SelectedItem = newTab;
            }

            newTab.Header = GetCorrectTitle(item.DocumentTitle);

            newTab.IconSource = string.IsNullOrWhiteSpace(item.FaviconUrl)
                ? UIConstants.BlankPageIcon
                : IconSource.GetFromUrl(item.FaviconUrl);

            if (item.Url == null || item.Url == Constants.AboutBlankUri.ToString())
            {
                return;
            }

            appTab.ViewModel?.SearchBarText = item.Url;
        }
    }

    private static void SubscribeToTabChanges(AppTab appTab, CompositeDisposable appTabDisposable, TabViewItem newTab)
    {
        appTab.ViewModel?.IsLoading
               .Subscribe(isLoading =>
               {
                   if (isLoading)
                   {
                       newTab.IconSource = loadingPageIcon;
                   }

                   appTab.ViewModel?.FaviconUrl.Take(1).Subscribe(faviconUrl => newTab.IconSource = IconSource.GetFromUrl(faviconUrl));
                   appTab.ViewModel?.DocumentTitle.Take(1).Subscribe(title => newTab.Header = GetCorrectTitle(title));
               });

        appTab.ViewModel?.FaviconUrl.Subscribe(faviconUrl => newTab.IconSource = IconSource.GetFromUrl(faviconUrl));
        appTab.ViewModel?.DocumentTitle.Subscribe(title => newTab.Header = GetCorrectTitle(title));
    }

    private static string GetCorrectTitle(string? title)
    {
        return string.IsNullOrWhiteSpace(title) || title.Equals(Constants.AboutBlankUri.ToString(), StringComparison.Ordinal)
            ? newTabTitle
            : title;
    }
}
