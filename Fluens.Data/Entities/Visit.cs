namespace Fluens.Data.Entities;

public class Visit
{
    public int Id { get; set; }
    public int PlaceId { get; set; }
    public Place Place { get; set; } = null!;
    public DateTime VisitDate { get; set; } = DateTime.UtcNow;
    public string? Referrer { get; set; }
    public TransitionType TransitionType { get; set; }
}

public enum TransitionType
{
    Typed,
    Link,
    Bookmark,
    Redirect,
    Embed,
}
