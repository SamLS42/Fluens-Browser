using Microsoft.EntityFrameworkCore;

namespace ArgyllBrowse.Data.Entities;

[Index(nameof(Url), IsUnique = true)]
[Index(nameof(LastVisitedOn))]
public class HistoryEntry
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public required string FaviconUrl { get; set; }
    public string? DocumentTitle { get; set; }

    /// <summary>
    /// Convert the values to UTC before saving and convert back to the appropriate time zone when using them.
    /// </summary>
    public DateTime LastVisitedOn { get; set; }
    public string? Host { get; set; }
}
