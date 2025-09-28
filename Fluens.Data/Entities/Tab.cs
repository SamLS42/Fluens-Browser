using System.ComponentModel.DataAnnotations;

namespace Fluens.Data.Entities;

public class Tab
{
    public int Id { get; set; }

    [Range(0, int.MaxValue)]
    public int Index { get; set; }
    public bool IsSelected { get; set; }
    public DateTime? ClosedOn { get; set; }
    public int BrowserWindowId { get; set; }
    public BrowserWindow BrowserWindow { get; set; } = null!;
    public int? PlaceId { get; set; }
    public Place? Place { get; set; }
}
