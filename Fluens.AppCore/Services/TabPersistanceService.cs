using Fluens.Data;
using Fluens.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fluens.AppCore.Services;
public class TabPersistencyService(IDbContextFactory<BrowserDbContext> dbContextFactory)
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

    public async Task<int> CreateTabAsync(Uri url)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        BrowserTab entity = new() { Url = url.ToString() };

        dbContext.OpenTabs.Add(entity);

        await dbContext.SaveChangesAsync();

        return entity.Id;
    }

    public async Task SetTabIndexAsync(int id, int index)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.OpenTabs
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(u => u.SetProperty(t => t.Index, index));
    }

    public async Task SetTabUrlAsync(int id, Uri url)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.OpenTabs
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(u => u.SetProperty(t => t.Url, url.ToString()));
    }

    public async Task SetIsTabSelectedAsync(int id, bool isTabSelected)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.OpenTabs
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(u => u.SetProperty(t => t.IsTabSelected, isTabSelected));
    }

    public async Task SaveTabFaviconUrlAsync(int id, string faviconUrl)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.OpenTabs
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(u => u.SetProperty(t => t.FaviconUrl, faviconUrl));
    }

    public async Task SaveTabDocumentTitleAsync(int id, string documentTitle)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.OpenTabs
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(u => u.SetProperty(t => t.DocumentTitle, documentTitle));
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
