namespace LostAndFound.Models;

public class User
{
    public int UserId { get; set; }
    public required string Login { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public required string? Email { get; set; }
    public required string? Phone { get; set; }
    public required int RoleId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    public Role? Role { get; set; }
}
