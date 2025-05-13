using System.Security.Cryptography;
using System.Text;
using LostAndFound.Data;
using LostAndFound.Models;

namespace LostAndFound.Services;

public class UserService(
    UserRepository userRepository,
    ActionLogRepository actionLogRepository)
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
        
        var passwordHash = HashPassword(password);
        
        if (user.PasswordHash != passwordHash)
        {
            return null;
        }
        
        await actionLogRepository.AddAsync(new ActionLog
        {
            UserId = user.UserId,
            ActionType = "Авторизация",
            Description = "Пользователь авторизовался",
            Timestamp = DateTime.Now
        });
        
        return user;
    }

    public async Task<int> AddUserAsync(User user, string password, int loggedInUserId)
    {
        user.PasswordHash = HashPassword(password);
        
        user.IsActive = true;
        user.CreatedDate = DateTime.Now;
        
        var userId = await userRepository.AddAsync(user);
        
        await actionLogRepository.AddAsync(new ActionLog
        {
            UserId = loggedInUserId,
            ActionType = "Новый пользователь",
            Description = $"Добавлен пользователь: {user.FullName}",
            Timestamp = DateTime.Now
        });
        
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
            await actionLogRepository.AddAsync(new ActionLog
            {
                UserId = loggedInUserId,
                ActionType = "Обновление пользователя",
                Description = $"Пользователь обновлен: {user.FullName}",
                Timestamp = DateTime.Now
            });
        }
        
        return result;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string newPassword, int loggedInUserId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return false;
        }
        
        user.PasswordHash = HashPassword(newPassword);
        
        var result = await userRepository.UpdateAsync(user);
        
        if (result)
        {
            await actionLogRepository.AddAsync(new ActionLog
            {
                UserId = loggedInUserId,
                ActionType = "Смена пароля",
                Description = $"Пароль изменен: {user.FullName}",
                Timestamp = DateTime.Now
            });
        }
        
        return result;
    }

    public async Task<bool> DeactivateUserAsync(int userId, int loggedInUserId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return false;
        }
        
        if (userId == loggedInUserId)
        {
            return false;
        }
        
        user.IsActive = false;
        
        var result = await userRepository.UpdateAsync(user);
        
        if (result)
        {
            await actionLogRepository.AddAsync(new ActionLog
            {
                UserId = loggedInUserId,
                ActionType = "Деактивация пользователя",
                Description = $"Пользователь деактивирован: {user.FullName}",
                Timestamp = DateTime.Now
            });
        }
        
        return result;
    }
    
    private static string HashPassword(string password)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var hash = Convert.ToHexStringLower(hashBytes);
        return hash;
    }
} 