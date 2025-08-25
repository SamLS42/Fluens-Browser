using Fluens.AppCore.Helpers;
using Fluens.AppCore.Services;
using Fluens.Data.Entities;
using System.Reactive.Linq;

namespace Fluens.AppCore.ViewModels;
public class AppPageViewModel(TabPersistencyService dataService)
{
    public async Task<AppTabViewModel[]> GetOpenTabsAsync()
    {
        BrowserTab[] tabs = await dataService.GetOpenTabsAsync();

        return [.. tabs.Select(item => new AppTabViewModel(item.Id, new Uri(item.Url), item.IsTabSelected, item.Index, item.DocumentTitle, item.FaviconUrl))];
    }

    public async Task<int> GetNewTabId()
    {
        return await dataService.CreateTabAsync(Constants.AboutBlankUri);
    }

    public async Task DeleteTabAsync(AppTabViewModel tab)
    {
        await dataService.DeleteTabAsync(tab.Id);
    }
}
