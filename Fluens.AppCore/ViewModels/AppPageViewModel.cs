using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.Services;
using Fluens.Data.Entities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Fluens.AppCore.ViewModels;

public class AppPageViewModel : IDisposable
{
    private readonly TabPersistencyService dataService = ServiceLocator.GetRequiredService<TabPersistencyService>();

    private readonly Subject<Unit> hasNoTabs = new();
    public IObservable<Unit> HasNoTabs => hasNoTabs.AsObservable();

    private readonly SourceCache<AppTabViewModel, int> tabsSource = new(vm => vm.Id);

    public ObservableCollection<AppTabViewModel> TabsSource { get; } = []; //For some reason, adding tabs is faster (visually) when using TabItemsSource instead of using Items directly
    public int WindowId { get; set; }

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
            });
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

    public async Task CloseTabAsync(int id)
    {
        await dataService.CloseTabAsync(id);
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
