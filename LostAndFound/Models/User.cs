namespace LostAndFound.Models;

public class User
{
    public int UserId { get; init; }
    public required string Login { get; init; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; init; }
    public required string? Email { get; init; }
    public required string? Phone { get; init; }
    public required int RoleId { get; init; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public Role? Role { get; set; }
} 