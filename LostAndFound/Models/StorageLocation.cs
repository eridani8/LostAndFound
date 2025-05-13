namespace LostAndFound.Models;

public class StorageLocation
{
    public int StorageLocationId { get; set; }
    public required string LocationName { get; set; }
    public required string Address { get; set; }
    public string? ContactPhone { get; set; }
    public string? WorkingHours { get; set; }
} 