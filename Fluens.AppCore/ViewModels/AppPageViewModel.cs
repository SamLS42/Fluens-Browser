using DynamicData;
using Fluens.AppCore.Enums;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.Services;
using Fluens.Data.Entities;
using System.Reactive;
using System.Reactive.Linq;

namespace Fluens.AppCore.ViewModels;
public class AppPageViewModel(TabPersistencyService dataService) : IDisposable
{
    private readonly SourceCache<AppTabViewModel, int> Tabs = new(t => t.Id);
    public IObservableCache<AppTabViewModel, int> TabsSource => Tabs.AsObservableCache();
    public IObservable<Unit> HasNoTabs => TabsSource.CountChanged
        .Skip(1)
        .Where(c => c == 0)
        .Select(_ => Unit.Default);

    public async Task RestoreOpenTabsAsync()
    {
        BrowserTab[] tabs = await dataService.GetOpenTabsAsync();

        foreach (BrowserTab item in tabs)
        {
            Tabs.Edit(updateAction =>
            {
                updateAction.AddOrUpdate(new AppTabViewModel(item.Id, new Uri(item.Url), item.IsTabSelected, item.Index, item.DocumentTitle, item.FaviconUrl));
            });
        }
    }

    public async Task AddBlankTabAsync()
    {
        int id = await dataService.CreateTabAsync(Constants.AboutBlankUri);

        Tabs.AddOrUpdate(new AppTabViewModel(id, Constants.AboutBlankUri));
    }

    public async Task CloseTabAsync(AppTabViewModel tab)
    {
        Tabs.Remove(tab);
        await dataService.DeleteTabAsync(tab.Id);
        tab.Dispose();
    }

    public async Task ApplyOnStartupSettingAsync(OnStartupSetting onStartupSetting)
    {
        switch (onStartupSetting)
        {
            case OnStartupSetting.OpenNewTab:
                await AddBlankTabAsync();
                break;
            case OnStartupSetting.RestoreOpenTabs:
                await RestoreOpenTabsAsync();
                break;
            //TODO
            //case OnStartupSetting.OpenSpecificTabs:
            //    break;
            case OnStartupSetting.RestoreAndOpenNewTab:
                await RestoreOpenTabsAsync();
                await AddBlankTabAsync();
                break;
            default:
                await AddBlankTabAsync();
                break;
        }

        if (TabsSource.Count == 0)
        {
            await AddBlankTabAsync();
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
            Tabs.Dispose();
        }
    }
}
