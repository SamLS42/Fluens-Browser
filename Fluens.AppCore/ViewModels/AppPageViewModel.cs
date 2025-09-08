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
    private readonly TabPersistencyService dataService = ServiceLocator.GetRequiredService<TabPersistencyService>();

    private readonly Subject<Unit> hasNoTabs = new();
    public IObservable<Unit> HasNoTabs => hasNoTabs.AsObservable();

    private readonly SourceCache<AppTabViewModel, int> tabsSource = new(vm => vm.Id);

    public ObservableCollection<AppTabViewModel> TabsSource { get; } = []; //For some reason, adding tabs is faster (visually) when using TabItemsSource instead of using Items directly
    public int WindowId { get; set; }

    [Reactive]
    public partial AppTabViewModel SelectedItem { get; set; } = null!;

    public AppPageViewModel()
    {
        Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(TabsSource, nameof(TabsSource.CollectionChanged))
            .Subscribe(_ =>
            {
                if (TabsSource.Count == 0)
                {
                    hasNoTabs.OnNext(Unit.Default);
                }

                tabsSource.EditDiff(TabsSource, areItemsEqual: (i1, i2) => i1.Id == i2.Id);

                UpdateTabIndexes();
            });

        this.WhenAnyValue(x => x.SelectedItem)
            .WhereNotNull()
            .Subscribe(_ =>
            {
                SelectedItem.IsSelected = true;

                foreach (AppTabViewModel item in TabsSource.Except([SelectedItem]))
                {
                    item.IsSelected = false;
                }
            });
    }


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

        if (TabsSource.Count == 0)
        {
            await CreateNewTab();
        }
    }

    private async Task RecoverStateAsync()
    {
        foreach (AppTabViewModel vm in await RecoverTabsAsync())
        {
            CreateTabViewItem(vm);
        }

        SelectedItem = TabsSource.First(item => item.IsSelected);
    }

    private async Task CreateNewTab()
    {
        AppTabViewModel vm = await CreateTabAsync();
        CreateTabViewItem(vm);
        SelectedItem = vm;
    }

    private async Task RestoreClosedTabAsync()
    {
        AppTabViewModel? vm = await GetClosedTabAsync();

        if (vm is null)
        {
            return;
        }

        SelectedItem = vm;
    }

    private void UpdateTabIndexes()
    {
        foreach (AppTabViewModel vm in TabsSource)
        {
            vm.Index = TabsSource.IndexOf(vm);
        }
    }

    public void CreateTabViewItem(AppTabViewModel vm)
    {
        if (vm.Index != null)
        {
            TabsSource.Insert(vm.Index.Value, vm);
        }
        else
        {
            TabsSource.Add(vm);
        }
    }

    public async Task CloseTabAsync(AppTabViewModel vm)
    {
        TabsSource.Remove(vm);
        await dataService.CloseTabAsync(vm.Id);
        vm.Dispose();
    }

    public bool HasTab(AppTabViewModel tab)
    {
        return TabsSource.Any(t => t == tab);
    }

    public async Task<AppTabViewModel[]> RecoverTabsAsync()
    {
        BrowserTab[] tabs = await dataService.RecoverTabsAsync();

        return [.. tabs.Select(tab => tab.ToAppTabViewModel(WindowId))];
    }

    public async Task<AppTabViewModel?> GetClosedTabAsync()
    {
        BrowserTab? tab = await dataService.GetClosedTabAsync();

        if (tab == null)
        {
            return null;
        }

        return tab.ToAppTabViewModel(WindowId);
    }

    public async Task<int> GetNewTabId()
    {
        return await dataService.CreateTabAsync(Constants.AboutBlankUri, WindowId);
    }

    public async Task<AppTabViewModel> CreateTabAsync(Uri? uri = null)
    {
        int id = await GetNewTabId();

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
        CreateTabViewItem(vm);
        SelectedItem = vm;
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
                SelectedItem.Refresh.Execute().Subscribe();
                break;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool dispose)
    {
        if (dispose)
        {
            hasNoTabs.OnCompleted();
            hasNoTabs.Dispose();
            tabsSource.Dispose();
        }
    }
}
