using ArgyllBrowse.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArgyllBrowse.Data;
public class BrowserDbContext(DbContextOptions<BrowserDbContext> options) : DbContext(options)
{
    public DbSet<BrowserTab> OpenTabs { get; set; }
}
