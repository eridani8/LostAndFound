namespace LostAndFound.Models;

public class ActionLog
{
    public int LogId { get; init; }
    public int UserId { get; set; }
    public DateTime ActionDate { get; init; } = DateTime.Now;
    public required string ActionType { get; init; }
    public string? Details { get; init; }
    public string? IpAddress { get; init; }
    
    public User? User { get; set; }
} 