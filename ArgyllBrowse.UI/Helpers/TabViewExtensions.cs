using ArgyllBrowse.AppCore.Helpers;
using ArgyllBrowse.Data.Entities;
using ArgyllBrowse.UI.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ArgyllBrowse.UI.Helpers;
internal static class TabViewExtensions
{
    private const string newTabTitle = "New Tab";
    private static readonly FontIconSource loadingPageIcon = new() { Glyph = "\uF16A" };
    private static readonly FontIconSource blankPageIcon = new() { Glyph = "\uE909" };

    internal static void AddEmptyTab(this TabView tabView)
    {
        ArgumentNullException.ThrowIfNull(tabView);

        AppTab appTab = new();
        CompositeDisposable appTabDisposable = [];

        TabViewItem newTab = new()
        {
            Header = newTabTitle,
            IconSource = blankPageIcon,
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
                ? blankPageIcon
                : GetTabIconSource(item.FaviconUrl);

            if (item.Url == null || item.Url == Constants.AboutBlankUri.ToString())
            {
                return;
            }

            appTab.ViewModel?.SearchBarText = item.Url;
        }
    }

    private static void SubscribeToTabChanges(AppTab appTab, CompositeDisposable appTabDisposable, TabViewItem newTab)
    {
        appTab.ViewModel?.Isloading
               .Subscribe(isLoading =>
               {
                   if (isLoading)
                   {
                       newTab.IconSource = loadingPageIcon;
                   }

                   appTab.ViewModel?.FaviconUrl.Take(1).Subscribe(faviconUrl => newTab.IconSource = GetTabIconSource(faviconUrl));
                   appTab.ViewModel?.DocumentTitle.Take(1).Subscribe(title => newTab.Header = GetCorrectTitle(title));
               });

        appTab.ViewModel?.FaviconUrl.Subscribe(faviconUrl => newTab.IconSource = GetTabIconSource(faviconUrl));
        appTab.ViewModel?.DocumentTitle.Subscribe(title => newTab.Header = GetCorrectTitle(title));
    }

    private static string GetCorrectTitle(string? title)
    {
        return string.IsNullOrWhiteSpace(title) || title.Equals(Constants.AboutBlankUri.ToString(), StringComparison.Ordinal)
            ? newTabTitle
            : title;
    }

    private static IconSource GetTabIconSource(string faviconUrl)
    {
        if (string.IsNullOrWhiteSpace(faviconUrl))
        {
            return blankPageIcon;
        }

        Uri faviconUri = new(faviconUrl);

        return faviconUri.AbsolutePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
            ? new ImageIconSource() { ImageSource = new SvgImageSource(faviconUri) }
            : new ImageIconSource() { ImageSource = new BitmapImage(faviconUri) };
    }
}
