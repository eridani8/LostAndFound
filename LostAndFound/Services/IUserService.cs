using LostAndFound.Models;

namespace LostAndFound.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    
    Task<User?> GetUserByIdAsync(int id);
    
    Task<User?> AuthenticateAsync(string login, string password);
    
    Task<int> AddUserAsync(User user, string password, int loggedInUserId);
    
    Task<bool> UpdateUserAsync(User user, int loggedInUserId);
    
    Task<bool> ChangePasswordAsync(int userId, string newPassword, int loggedInUserId);
    
    Task<bool> DeactivateUserAsync(int userId, int loggedInUserId);
} 