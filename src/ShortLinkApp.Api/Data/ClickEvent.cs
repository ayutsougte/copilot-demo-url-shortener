namespace ShortLinkApp.Api.Data;

public class ClickEvent
{
    public int Id { get; set; }
    public int LinkId { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    public string? Referrer { get; set; }

    public Link Link { get; set; } = null!;
}
