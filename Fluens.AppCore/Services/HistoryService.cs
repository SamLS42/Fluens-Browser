using Fluens.AppCore.Helpers;
using Fluens.Data;
using Fluens.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System.Collections.ObjectModel;
using Toimik.UrlNormalization;

namespace Fluens.AppCore.Services;

public class HistoryService(IDbContextFactory<BrowserDbContext> dbContextFactory, HttpUrlNormalizer httpUrlNormalizer)
{
    public async Task AddEntryAsync(Uri url, string faviconUrl, string documentTitle, CancellationToken cancellationToken = default)
    {
        if (url == Constants.AboutBlankUri)
        {
            return;
        }

        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        string normilizedUrl = httpUrlNormalizer.Normalize(url.ToString());

        //Get or create place
        Place place = await dbContext.Places.SingleOrDefaultAsync(e => e.NormalizedUrl == normilizedUrl, cancellationToken: cancellationToken)
            ?? (await dbContext.Places.AddAsync(new Place()
            {
                Url = url.ToString(),
                NormalizedUrl = normilizedUrl,
                FaviconUrl = faviconUrl,
                Path = url.AbsolutePath,
                Title = documentTitle,
                Hostname = url.Host,
            }, cancellationToken)).Entity;

        //Create Visit
        await dbContext.Visits.AddAsync(new()
        {
            Place = place,
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<HistoryPage> GetEntriesAsync(DateTime? lastDate = null, int? lastId = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Fetch the items and determine if there are more
        Place[] items = await dbContext.Places
            .OrderByDescending(x => x.LastVisitDate)
            .ThenByDescending(x => x.Id) // tie-breaker
            .Where(e =>
                lastDate == null ||
                (e.LastVisitDate < lastDate.Value) ||
                (e.LastVisitDate == lastDate.Value && e.Id < lastId))
            .Take(limit + 1)
            .ToArrayAsync(cancellationToken);

        // Extract the cursor and ID for the next page
        bool hasMore = items.Length > limit;
        int? nextLastId = hasMore ? items[^1].Id : null;
        DateTime? nextLastDate = hasMore ? items[^1].LastVisitDate : null;

        foreach (Place item in items)
        {
            item.LastVisitDate = item.LastVisitDate.ToLocalTime();
        }

        return new HistoryPage()
        {
            Items = new ReadOnlyCollection<Place>(hasMore ? [.. items.SkipLast(1)] : items),
            NextLastDate = nextLastDate,
            NextLastId = nextLastId
        };
    }

    internal async Task DeleteEntriesAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.Visits.Where(e => ids.Contains(e.Id)).ExecuteDeleteAsync(cancellationToken);
    }

    internal async Task ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        await using BrowserDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.Visits.ExecuteDeleteAsync(cancellationToken);
    }
}
