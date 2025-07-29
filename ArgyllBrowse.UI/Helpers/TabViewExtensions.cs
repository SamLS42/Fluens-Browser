using ArgyllBrowse.UI.ViewModels.Helpers;
using ArgyllBrowse.UI.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgyllBrowse.UI.Helpers;
internal static class TabViewExtensions
{
    private static readonly FontIconSource loadingPageIcon = new() { Glyph = "\uF16A" };
    private static readonly FontIconSource blankPageIcon = new() { Glyph = "\uE909" };

    internal static void AddNewAppTab(this TabView tabView, Uri? uri = null, bool isSelected = true)
    {
        ArgumentNullException.ThrowIfNull(tabView);

        AppTab appTab = new();
        CompositeDisposable appTabDisposable = [];

        TabViewItem newTab = new()
        {
            Header = "New Tab",
            IconSource = blankPageIcon,
            Content = appTab
        };

        tabView.TabItems.Add(newTab);

        if (isSelected)
        {
            tabView.SelectedItem = newTab;
        }

        SubscribeToTabChanges(appTab, appTabDisposable, newTab);

        if (uri == null)
        {
            return;
        }

        appTab.ViewModel?.Url = uri;
    }

    private static void SubscribeToTabChanges(AppTab appTab, CompositeDisposable appTabDisposable, TabViewItem newTab)
    {
        appTab.ViewModel?.DocumentTitleChanges
            .Subscribe(title => newTab.Header = title.Equals(Constants.AboutBlankUri, StringComparison.Ordinal) ? "New Tab" : title)
            .DisposeWith(appTabDisposable);

        appTab.ViewModel?.NavigationStarting.Skip(1)
            .Subscribe(_ => newTab.IconSource = loadingPageIcon)
            .DisposeWith(appTabDisposable);

        appTab.ViewModel?.NavigationCompleted
            .SelectMany(_ => appTab.ViewModel!.FaviconUrl.DelaySubscription(TimeSpan.FromMilliseconds(100)).ObserveOn(RxApp.MainThreadScheduler).Take(1))
            .Subscribe(faviconUrl => newTab.IconSource = GetTabIconSource(faviconUrl))
            .DisposeWith(appTabDisposable);

        appTab.ViewModel?.Disposed
            .Subscribe(_ => appTabDisposable.Dispose())
            .DisposeWith(appTabDisposable);
    }

    private static IconSource GetTabIconSource(string? faviconUrl)
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
