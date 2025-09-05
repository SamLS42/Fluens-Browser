namespace Fluens.Data.Entities;

public class BrowserWindow
{
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsMaximized { get; set; }
    public DateTime? ClosedOn { get; set; }
    public ICollection<BrowserTab> Tabs { get; } = [];
}
