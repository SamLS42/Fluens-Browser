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
}
