namespace Fluens.Data.Entities;

public class Bookmark
{
    public int Id { get; set; }
    public int PlaceId { get; set; }
    public Place Place { get; set; } = null!;
    public string? Folder { get; set; }
    public DateTime AddedDate { get; set; }
    public string? Name { get; set; }
    public string? Notes { get; set; }
}
