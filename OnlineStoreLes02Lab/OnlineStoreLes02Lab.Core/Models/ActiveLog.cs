namespace OnlineStoreLes02Lab.Core.Models;

public class ActiveLog
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ActionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}