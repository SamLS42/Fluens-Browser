using ArgyllBrowse.Data.Entities;
using ArgyllBrowse.Data.Services;

namespace ArgyllBrowse.AppCore.ViewModels;
public class AppPageViewModel(BrowserDataService dataService)
{
    public async Task<BrowserTab[]> GetOpenTabsAsync()
    {
        return await dataService.GetOpenTabsAsync();
    }

    public async Task DeleteTabAsync(int id)
    {
        await dataService.DeleteTabAsync(id);
    }
}
