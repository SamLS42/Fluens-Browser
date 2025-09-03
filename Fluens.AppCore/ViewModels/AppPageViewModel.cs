using Fluens.AppCore.Helpers;
using Fluens.AppCore.Services;
using Fluens.Data.Entities;
using System.Reactive.Linq;

namespace Fluens.AppCore.ViewModels;
public class AppPageViewModel(TabPersistencyService dataService)
{
    public async Task<AppTabViewModel[]> RecoverTabsAsync()
    {
        BrowserTab[] tabs = await dataService.GetOpenTabsAsync();

        return [.. tabs.Select(item => new AppTabViewModel(item.Id, new Uri(item.Url), item.IsTabSelected, item.Index, item.DocumentTitle, item.FaviconUrl))];
    }

    public async Task<AppTabViewModel?> RecoverTabAsync()
    {
        BrowserTab? tab = await dataService.GetClosedTabAsync();

        if (tab == null)
        {
            return null;
        }

        return new AppTabViewModel(tab.Id, new Uri(tab.Url), tab.IsTabSelected, tab.Index, tab.DocumentTitle, tab.FaviconUrl);
    }

    public async Task<int> GetNewTabId()
    {
        return await dataService.CreateTabAsync(Constants.AboutBlankUri);
    }

    public async Task CloseTabAsync(int id)
    {
        await dataService.CloseTabAsync(id);
    }

    public async Task<AppTabViewModel> CreateTabAsync(Uri? uri = null, bool isSelected = true)
    {
        int id = await GetNewTabId();
        AppTabViewModel vm = new(id, uri ?? Constants.AboutBlankUri, isSelected);
        return vm;
    }
}
