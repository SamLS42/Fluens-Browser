using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace Fluens.Data.Entities;

[Index(nameof(NormalizedUrl), IsUnique = true)]
public class Place
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public required string NormalizedUrl { get; set; }
    public required string FaviconUrl { get; set; }
    public required string Hostname { get; set; }
    public required string Path { get; set; }
    public required string Title { get; set; }
    public int VisitCount { get; set; }
    public DateTime LastVisitDate { get; set; } = DateTime.UtcNow;
    public int TypedCount { get; set; }
    public bool IsBookmarked { get; set; }
    public Collection<string> Tags { get; } = [];
}
