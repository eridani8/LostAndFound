namespace LostAndFound.Models;

public class LostItem
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime FoundDate { get; set; }
    public string FoundLocation { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int StorageLocationId { get; set; }
    public string Status { get; set; } = "Ожидание";
    public string? ImagePath { get; set; }
    public int RegisteredBy { get; set; }
    public DateTime RegistrationDate { get; set; }
    
    public Category? Category { get; set; }
    public StorageLocation? StorageLocation { get; set; }
    public User? RegisteredByUser { get; set; }
} 