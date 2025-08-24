using Fluens.AppCore.Helpers;
using Fluens.Data;
using Fluens.Data.Entities;
using Microsoft.EntityFrameworkCore;
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

    public async Task<HistoryPage> GetHistoryAsync(int? lastId = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Fetch the items and determine if there are more
        var items = await dbContext.History
            .Where(e => lastId == null || e.Id > lastId)
            .OrderByDescending(e => e.LastVisitedOn)
            .Take(limit + 1)
            .ToArrayAsync(cancellationToken);

        // Extract the cursor and ID for the next page
        bool hasMore = items.Length > limit;
        int? nextLastId = hasMore ? items[^1].Id : null;

        return new HistoryPage()
        {
            Items = new ReadOnlyCollection<HistoryEntry>(hasMore ? [.. items.SkipLast(1)] : items),
            NextLastId = nextLastId,
            HasMore = hasMore
        };
    }
}
