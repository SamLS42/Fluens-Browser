using Fluens.AppCore.Helpers;
using Fluens.Data;
using Fluens.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Fluens.AppCore.Services;

public class HistoryService(IDbContextFactory<BrowserDbContext> dbContextFactory)
{
    public async Task AddEntryAsync(Uri url, string faviconUrl, string documentTitle, CancellationToken cancellationToken = default)
    {
        if (url == Constants.AboutBlankUri)
        {
            return;
        }

        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        HistoryEntry existentEntry = await dbContext.History.SingleOrDefaultAsync(e => e.Url == url.ToString(), cancellationToken: cancellationToken)
            ?? (await dbContext.History.AddAsync(new HistoryEntry()
            {
                Url = url.ToString(),
                FaviconUrl = faviconUrl,
                DocumentTitle = documentTitle,
                Host = url.Host,
            }, cancellationToken)).Entity;

        existentEntry.LastVisitedOn = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<HistoryPage> GetEntriesAsync(DateTime? lastDate = null, int? lastId = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Fetch the items and determine if there are more
        HistoryEntry[] items = await dbContext.History
            .OrderByDescending(x => x.LastVisitedOn)
            .ThenByDescending(x => x.Id) // tie-breaker
            .Where(e =>
                lastDate == null ||
                (e.LastVisitedOn < lastDate.Value) ||
                (e.LastVisitedOn == lastDate.Value && e.Id < lastId))
            .Take(limit + 1)
            .ToArrayAsync(cancellationToken);

        // Extract the cursor and ID for the next page
        bool hasMore = items.Length > limit;
        int? nextLastId = hasMore ? items[^1].Id : null;
        DateTime? nextLastDate = hasMore ? items[^1].LastVisitedOn : null;

        foreach (HistoryEntry item in items)
        {
            item.LastVisitedOn = item.LastVisitedOn.ToLocalTime();
        }

        return new HistoryPage()
        {
            Items = new ReadOnlyCollection<HistoryEntry>(hasMore ? [.. items.SkipLast(1)] : items),
            NextLastDate = nextLastDate,
            NextLastId = nextLastId
        };
    }

    internal async Task DeleteEntriesAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.History.Where(e => ids.Contains(e.Id)).ExecuteDeleteAsync(cancellationToken);
    }

    internal async Task ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.History.ExecuteDeleteAsync(cancellationToken);
    }
}
