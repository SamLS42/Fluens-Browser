using ArgyllBrowse.Data.Entities;
using ArgyllBrowse.Data.Services;
using ArgyllBrowse.UI.Views;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ArgyllBrowse.UI.ViewModels;
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
