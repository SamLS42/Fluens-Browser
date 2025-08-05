namespace ArgyllBrowse.Data.Entities;
public class BrowserTab
{
    public int Id { get; set; }
    public int Index { get; set; }
    public required string Url { get; set; }
    public string? FaviconUrl { get; set; }
    public string? DocumentTitle { get; set; }

    public bool IsTabSelected { get; set; }
}
