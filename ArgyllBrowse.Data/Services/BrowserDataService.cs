using ArgyllBrowse.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgyllBrowse.Data.Services;
public class BrowserDataService(IDbContextFactory<BrowserDbContext> dbContextFactory)
{
    public async Task<BrowserTab[]> GetOpenTabsAsync()
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        BrowserTab[] openTabs = await dbContext.OpenTabs
            .OrderBy(t => t.Index)
            .AsNoTracking()
            .ToArrayAsync();

        return openTabs;
    }

    public BrowserTab[] GetOpenTabs()
    {
        using BrowserDbContext dbContext = dbContextFactory.CreateDbContext();

        BrowserTab[] openTabs = [.. dbContext.OpenTabs.OrderBy(t => t.Index).AsNoTracking()];

        return openTabs;
    }

    public int CreateTab(Uri url)
    {
        using BrowserDbContext dbContext = dbContextFactory.CreateDbContext();

        ArgumentNullException.ThrowIfNull(url);

        BrowserTab entity = new() { Url = url.ToString() };

        dbContext.OpenTabs.Add(entity);

        dbContext.SaveChanges();

        return entity.Id;
    }

    public async Task SaveTabStateAsync(int id, int index, Uri url, bool isTabSelected, string faviconUrl, string documentTitle)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.OpenTabs
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(u => u
                .SetProperty(t => t.Index, index)
                .SetProperty(t => t.Url, url.ToString())
                .SetProperty(t => t.IsTabSelected, isTabSelected)
                .SetProperty(t => t.FaviconUrl, faviconUrl)
                .SetProperty(t => t.DocumentTitle, documentTitle)
                );
    }

    public async Task DeleteTabAsync(int id)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.OpenTabs.Where(t => t.Id == id).ExecuteDeleteAsync();
    }

    public async Task ClearOpenTabsAsync()
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.OpenTabs.ExecuteDeleteAsync();
    }
}
