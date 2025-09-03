using Fluens.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fluens.Data;
public class BrowserDbContext(DbContextOptions<BrowserDbContext> options) : DbContext(options)
{
    public DbSet<BrowserTab> Tabs { get; set; }
    public DbSet<HistoryEntry> History { get; set; }
}
