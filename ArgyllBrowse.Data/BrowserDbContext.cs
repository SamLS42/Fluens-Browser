using ArgyllBrowse.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgyllBrowse.Data;
public class BrowserDbContext(DbContextOptions<BrowserDbContext> options) : DbContext(options)
{
    public DbSet<BrowserTab> OpenTabs { get; set; }
}
