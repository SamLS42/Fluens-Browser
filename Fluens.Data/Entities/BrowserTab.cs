using System.ComponentModel.DataAnnotations;

namespace Fluens.Data.Entities;

public class BrowserTab
{
    public int Id { get; set; }

    [Range(0, int.MaxValue)]
    public int Index { get; set; }
    public required string Url { get; set; }
    public string? FaviconUrl { get; set; }
    public string? DocumentTitle { get; set; }
    public DateTime? ClosedOn { get; set; }
    public bool IsSelected { get; set; }
    public int BrowserWindowId { get; set; }
    public BrowserWindow BrowserWindow { get; set; } = null!;
}
