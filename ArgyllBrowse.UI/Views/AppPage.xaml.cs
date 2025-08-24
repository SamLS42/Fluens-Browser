using ArgyllBrowse.AppCore.Enums;
using ArgyllBrowse.AppCore.Helpers;
using ArgyllBrowse.AppCore.ViewModels;
using ArgyllBrowse.UI.Helpers;
using DynamicData;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using WinRT;

namespace ArgyllBrowse.UI.Views;

public sealed partial class AppPage : ReactiveAppPage, IDisposable
{
    private readonly CompositeDisposable disposables = [];
    public UIElement TitleBar => CustomDragRegion;

    private readonly ReadOnlyObservableCollection<TabViewItem> tabItems;

    public AppPage()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<AppPageViewModel>();

        Observable.FromEventPattern<TabView, object>(tabView, nameof(tabView.AddTabButtonClick))
            .Subscribe(async _ => await ViewModel.AddBlankTabAsync());

        Observable.FromEventPattern<SelectionChangedEventArgs>(tabView, nameof(tabView.SelectionChanged))
            .Subscribe(ep =>
            {
                ep.EventArgs.RemovedItems.FirstOrDefault()?.As<TabViewItem>().ViewModel!.IsSelected = false;
                ep.EventArgs.AddedItems.FirstOrDefault()?.As<TabViewItem>().ViewModel!.IsSelected = true;
            });

        Observable.FromEventPattern<TabView, TabViewTabCloseRequestedEventArgs>(tabView, nameof(tabView.TabCloseRequested))
            .Subscribe(async pattern => await RemoveTabAsync(pattern));

        ViewModel.TabsSource.Connect()
            .Do(UpdateTabIndexes)
            .Transform(vm =>
            {
                AppTab appTab = new(vm);

                TabViewItem newTab = new()
                {
                    Header = UIConstants.NewTabTitle,
                    IconSource = UIConstants.BlankPageIcon,
                    Content = appTab
                };

                vm.IsLoading.Subscribe(isLoading =>
                {
                    if (isLoading)
                    {
                        newTab.IconSource = UIConstants.LoadingPageIcon;
                    }

                    vm.FaviconUrl.Take(1).Subscribe(faviconUrl => newTab.IconSource = IconSource.GetFromUrl(faviconUrl));
                    vm.DocumentTitle.Take(1).Subscribe(title => newTab.Header = GetCorrectTitle(title));
                });

                vm.FaviconUrl.Subscribe(faviconUrl => newTab.IconSource = IconSource.GetFromUrl(faviconUrl));
                vm.DocumentTitle.Subscribe(title => newTab.Header = GetCorrectTitle(title));

                return newTab;
            })
            .Bind(out tabItems)
            .Subscribe(changes =>
            {
                TabViewItem? lastAdded = changes.Where(change => change.Reason == ChangeReason.Add).LastOrDefault().Current;

                if (lastAdded is TabViewItem tb && tb.ViewModel.IsSelected)
                {
                    tabView.SelectedItem = lastAdded;
                }
            })
            .DisposeWith(disposables);

    }

    private static string GetCorrectTitle(string? title)
    {
        return string.IsNullOrWhiteSpace(title) || title.Equals(Constants.AboutBlankUri.ToString(), StringComparison.Ordinal)
            ? UIConstants.NewTabTitle
            : title;
    }

    private void UpdateTabIndexes(IChangeSet<AppTabViewModel, int> set)
    {
        foreach (TabViewItem tabItem in tabView.TabItems.OfType<TabViewItem>())
        {
            AppTab appTab = tabItem.Content.As<AppTab>();
            int newIndex = tabView.TabItems.IndexOf(tabItem);
            appTab.ViewModel?.Index = newIndex;
        }
    }

    private async Task RemoveTabAsync(EventPattern<TabView, TabViewTabCloseRequestedEventArgs> pattern)
    {
        AppTab tab = pattern.EventArgs.Tab.Content.As<AppTab>();
        await ViewModel!.CloseTabAsync(tab.ViewModel!);
        tab.Dispose();
    }

    internal void ApplyOnStartupSetting(OnStartupSetting onStartupSetting)
    {
        Observable.FromEventPattern<RoutedEventArgs>(tabView, nameof(tabView.Loaded))
            .Subscribe(async _ =>
            {
                switch (onStartupSetting)
                {
                    case OnStartupSetting.OpenNewTab:
                        await ViewModel!.AddBlankTabAsync();
                        break;
                    case OnStartupSetting.RestoreOpenTabs:
                        await ViewModel!.RestoreOpenTabsAsync();
                        break;
                    //TODO
                    //case OnStartupSetting.OpenSpecificTabs:
                    //    break;
                    case OnStartupSetting.RestoreAndOpenNewTab:
                        await ViewModel!.RestoreOpenTabsAsync();
                        await ViewModel!.AddBlankTabAsync();
                        break;
                    default:
                        await ViewModel!.AddBlankTabAsync();
                        break;
                }

                if (ViewModel.TabsSource.Count == 0)
                {
                    await ViewModel!.AddBlankTabAsync();
                }
            });
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}

public partial class ReactiveAppPage : ReactiveUserControl<AppPageViewModel>;