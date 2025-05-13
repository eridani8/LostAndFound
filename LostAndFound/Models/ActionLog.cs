namespace LostAndFound.Models;

public class ActionLog
{
    public int LogId { get; set; }
    public int UserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    
    public User? User { get; set; }
} 