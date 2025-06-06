using System.Security.Cryptography;
using System.Text;
using LostAndFound.Data;
using LostAndFound.Models;

namespace LostAndFound.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    
    Task<User?> GetUserByIdAsync(int id);
    
    Task<User?> AuthenticateAsync(string login, string password);
    
    Task<int> AddUserAsync(User user, string password, int loggedInUserId);
    
    Task<bool> UpdateUserAsync(User user, int loggedInUserId);
}

public class UserService(UserRepository userRepository, ActionLogRepository logRepository)
    : IUserService
{
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await userRepository.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await userRepository.GetByIdAsync(id);
    }

    public async Task<User?> AuthenticateAsync(string login, string password)
    {
        var user = await userRepository.GetByLoginAsync(login);

        if (user is not { IsActive: true })
        {
            return null;
        }

        var passwordHash = password.HashPassword();

        if (user.PasswordHash != passwordHash)
        {
            return null;
        }
        
        App.CurrentUser = user;

        await logRepository.AddAsync(
            new ActionLog
            {
                ActionType = "Login",
                Details = "Пользователь авторизовался"
            }
        );

        return user;
    }

    public async Task<int> AddUserAsync(User user, string password, int loggedInUserId)
    {
        user.PasswordHash = password.HashPassword();

        user.IsActive = true;
        user.CreatedDate = DateTime.Now;

        var userId = await userRepository.AddAsync(user);

        await logRepository.AddAsync(
            new ActionLog
            {
                ActionType = "AddUser",
                Details = $"Добавлен пользователь: {user.FullName}"
            }
        );

        return userId;
    }

    public async Task<bool> UpdateUserAsync(User user, int loggedInUserId)
    {
        var currentUser = await userRepository.GetByIdAsync(user.UserId);

        if (currentUser == null)
        {
            return false;
        }

        user.PasswordHash = currentUser.PasswordHash;
        user.CreatedDate = currentUser.CreatedDate;

        var result = await userRepository.UpdateAsync(user);

        if (result)
        {
            await logRepository.AddAsync(
                new ActionLog
                {
                    ActionType = "UpdateUser",
                    Details = $"Пользователь обновлен: {user.FullName}"
                }
            );
        }

        return result;
    }
}
