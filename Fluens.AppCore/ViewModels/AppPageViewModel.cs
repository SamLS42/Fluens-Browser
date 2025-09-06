using Fluens.AppCore.Helpers;
using Fluens.AppCore.Services;
using Fluens.Data.Entities;
using System.Reactive.Linq;

namespace Fluens.AppCore.ViewModels;

public class AppPageViewModel(TabPersistencyService dataService)
{
    public int WindowId { get; set; }
    public async Task<AppTabViewModel[]> RecoverTabsAsync()
    {
        BrowserTab[] tabs = await dataService.GetOpenTabsAsync();

        return [
            .. tabs.Select(item => new AppTabViewModel(item.Id, new Uri(item.Url), item.IsSelected, WindowId, item.DocumentTitle ?? Constants.NewTabTitle, item.FaviconUrl, item.Index))
            ];
    }

    public async Task<AppTabViewModel?> RecoverTabAsync()
    {
        BrowserTab? tab = await dataService.GetClosedTabAsync();

        if (tab == null)
        {
            return null;
        }

        return new AppTabViewModel(tab.Id, new Uri(tab.Url), tab.IsSelected, WindowId, tab.DocumentTitle ?? Constants.NewTabTitle, tab.FaviconUrl, tab.Index);
    }

    public async Task<int> GetNewTabId()
    {
        return await dataService.CreateTabAsync(Constants.AboutBlankUri, WindowId);
    }

    public async Task CloseTabAsync(int id)
    {
        await dataService.CloseTabAsync(id);
    }

    public async Task<AppTabViewModel> CreateTabAsync(Uri? uri = null, bool isSelected = true)
    {
        int id = await GetNewTabId();
        AppTabViewModel vm = new(id, uri ?? Constants.AboutBlankUri, isSelected, WindowId, Constants.NewTabTitle, string.Empty);
        return vm;
    }
}
