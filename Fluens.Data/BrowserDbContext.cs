using Fluens.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fluens.Data;

public class BrowserDbContext(DbContextOptions<BrowserDbContext> options) : DbContext(options)
{
    public DbSet<BrowserTab> Tabs { get; set; } = null!;
    public DbSet<Visit> Visits { get; set; } = null!;
    public DbSet<Place> Places { get; set; } = null!;
    public DbSet<BrowserWindow> BrowserWindows { get; set; } = null!;
}
