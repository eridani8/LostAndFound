namespace LostAndFound.Models;

public class ItemReturn
{
    public int ReturnId { get; set; }
    public int ItemId { get; set; }
    public string ReturnedTo { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string? ContactInfo { get; set; }
    public int ReceivedBy { get; set; }
    public string? Notes { get; set; }
    
    public LostItem? LostItem { get; set; }
    public User? ReceivedByUser { get; set; }
} 