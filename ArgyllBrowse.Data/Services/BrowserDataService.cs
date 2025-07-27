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
    public async Task<BrowserTab[]> GetOpenTabs()
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        BrowserTab[] openTabs = await dbContext.OpenTabs
            .AsNoTracking()
            .ToArrayAsync();

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

    public async Task<int> CreateTabAsync(Uri url)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        ArgumentNullException.ThrowIfNull(url);
        return (await dbContext.OpenTabs.AddAsync(new BrowserTab() { Url = url.ToString() })).Entity.Id;
    }

    public async Task SaveTabStateAsync(int id, int index, Uri url)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.OpenTabs
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(u => u
                .SetProperty(t => t.Index, index)
                .SetProperty(t => t.Url, url.ToString())
            );
    }

    public async Task DeleteTabAsync(int id)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.OpenTabs.Where(t => t.Id == id).ExecuteDeleteAsync();
    }
}
