using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArgyllBrowse.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BrowserDbContext>
{
    public BrowserDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<BrowserDbContext> optionsBuilder = new();

        optionsBuilder.UseSqlite("BrowserStorage.db");

        return new BrowserDbContext(optionsBuilder.Options);
    }
}
