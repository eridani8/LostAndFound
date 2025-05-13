namespace LostAndFound.Models;

public class ActionLog
{
    public int LogId { get; set; }
    public int UserId { get; set; }
    public DateTime ActionDate { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    
    public User? User { get; set; }
} 