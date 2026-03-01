namespace CopilotDemo.Shared;

public class ShortLink
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
