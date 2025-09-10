using DynamicData;
using Fluens.AppCore.Enums;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.Services;
using Fluens.Data.Entities;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Fluens.AppCore.ViewModels;

public partial class AppPageViewModel : ReactiveObject, IDisposable
{
    [Reactive]
    public partial IViewFor<AppTabViewModel> SelectedItem { get; set; } = null!;
    public IObservable<Unit> HasNoTabs => hasNoTabs.AsObservable();
    public ObservableCollection<IViewFor<AppTabViewModel>> Tabs { get; } = []; //For some reason, adding tabs is faster (visually) when using TabItemsSource instead of using Items directly
    public int WindowId { get; set; }
    private IViewForFactory ViewForFactory { get; } = ServiceLocator.GetRequiredService<IViewForFactory>();
    public AppPageViewModel()
    {
        Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(Tabs, nameof(Tabs.CollectionChanged))
            .Subscribe(_ =>
            {
                if (Tabs.Count == 0)
                {
                    hasNoTabs.OnNext(Unit.Default);
                }

                UpdateTabIndexes();
            });

        this.WhenAnyValue(x => x.SelectedItem)
            .WhereNotNull()
            .Subscribe(_ =>
            {
                foreach (IViewFor<AppTabViewModel> item in Tabs.Except([SelectedItem]))
                {
                    item.ViewModel!.IsSelected = false;
                }

                SelectedItem.ViewModel!.IsSelected = true;
            });
    }

    private readonly TabPersistencyService dataService = ServiceLocator.GetRequiredService<TabPersistencyService>();

    private readonly Subject<Unit> hasNoTabs = new();

    public async Task ApplyOnStartupSettingAsync(OnStartupSetting onStartupSetting)
    {
        switch (onStartupSetting)
        {
            case OnStartupSetting.OpenNewTab:
                await CreateNewTab();
                break;
            case OnStartupSetting.RestoreOpenTabs:
                await RecoverStateAsync();
                break;
            //TODO
            //case OnStartupSetting.OpenSpecificTabs:
            //    break;
            case OnStartupSetting.RestoreAndOpenNewTab:
                await RecoverStateAsync();
                await CreateNewTab();
                break;
            default:
                await CreateNewTab();
                break;
        }

        if (Tabs.Count == 0)
        {
            await CreateNewTab();
        }
    }

    public IViewFor<AppTabViewModel> CreateTabViewItem(AppTabViewModel vm)
    {
        IViewFor<AppTabViewModel> tab = ViewForFactory.CreateAppTab(vm);

        if (tab.ViewModel!.Index != null)
        {
            Tabs.Insert(tab.ViewModel!.Index.Value, tab);
        }
        else
        {
            Tabs.Add(tab);
        }

        return tab;
    }

    public async Task CloseTabAsync(IViewFor<AppTabViewModel> tab)
    {
        Tabs.Remove(tab);
        await dataService.CloseTabAsync(tab.ViewModel!.Id);
        tab.ViewModel!.Dispose();
    }

    public bool HasTab(AppTabViewModel tab)
    {
        return Tabs.Any(t => t == tab);
    }

    public async Task<AppTabViewModel> CreateTabAsync(Uri? uri = null)
    {
        int id = await dataService.CreateTabAsync(Constants.AboutBlankUri, WindowId);

        AppTabViewModel vm = new()
        {
            Id = id,
            Url = uri ?? Constants.AboutBlankUri,
            ParentWindowId = WindowId,
            DocumentTitle = Constants.NewTabTitle,
            FaviconUrl = string.Empty
        };

        return vm;
    }

    public async Task CreateNewTabAsync()
    {
        AppTabViewModel vm = await CreateTabAsync();
        var tab = CreateTabViewItem(vm);
        SelectedItem = tab;
    }

    public async Task HandleKeyboardShortcutAsync(ShortcutMessage message)
    {
        switch (message)
        {
            case { Ctrl: true, Shift: true, Key: "T" }:
                await RestoreClosedTabAsync();
                break;
            case { Ctrl: true, Key: "T" }:
                await CreateNewTab();
                break;
            case { Ctrl: true, Key: "W" }:
                await CloseTabAsync(SelectedItem);
                break;
            case { Key: "F5" }:
                SelectedItem.ViewModel!.Refresh.Execute().Subscribe();
                break;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private async Task RecoverStateAsync()
    {
        BrowserTab[] tabs = await dataService.RecoverTabsAsync();

        foreach (BrowserTab tab in tabs)
        {
            AppTabViewModel vm = tab.ToAppTabViewModel();
            vm.ParentWindowId = WindowId;
            CreateTabViewItem(vm);
        }

        SelectedItem = Tabs.FirstOrDefault(item => item.ViewModel!.IsSelected, Tabs.First());
    }

    private async Task CreateNewTab()
    {
        AppTabViewModel vm = await CreateTabAsync();
        var tab = CreateTabViewItem(vm);
        SelectedItem = tab;
    }

    private async Task RestoreClosedTabAsync()
    {
        BrowserTab? tabData = await dataService.GetClosedTabAsync();

        if (tabData == null)
        {
            return;
        }

        AppTabViewModel vm = tabData.ToAppTabViewModel();

        vm.ParentWindowId = WindowId;

        var tab = CreateTabViewItem(vm);

        SelectedItem = tab;
    }

    private void UpdateTabIndexes()
    {
        foreach (IViewFor<AppTabViewModel> tab in Tabs)
        {
            tab.ViewModel!.Index = Tabs.IndexOf(tab);
        }
    }

    protected virtual void Dispose(bool dispose)
    {
        if (dispose)
        {
            hasNoTabs.OnCompleted();
            hasNoTabs.Dispose();
        }
    }
}
