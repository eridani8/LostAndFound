using System.Data;
using Dapper;
using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Data;

public class UserRepository(IDatabaseConnectionFactory connectionFactory) : IRepository<User>
{
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                        SELECT u.*, r.RoleId AS Role_RoleId, r.*
                        FROM Users u
                        LEFT JOIN Roles r ON u.RoleId = r.RoleId
                        ORDER BY u.UserId DESC
            """;

        var userDictionary = new Dictionary<int, User>();

        await connection.QueryAsync<User, Role, User>(
            sql,
            (user, role) =>
            {
                if (!userDictionary.TryGetValue(user.UserId, out var existingUser))
                {
                    existingUser = user;
                    existingUser.Role = role;
                    userDictionary.Add(user.UserId, existingUser);
                }

                return existingUser;
            },
            splitOn: "Role_RoleId"
        );

        return userDictionary.Values;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                        SELECT u.*, r.RoleId AS Role_RoleId, r.*
                        FROM Users u
                        LEFT JOIN Roles r ON u.RoleId = r.RoleId
                        WHERE u.UserId = @Id
            """;

        var users = await connection.QueryAsync<User, Role, User>(
            sql,
            (user, role) =>
            {
                user.Role = role;
                return user;
            },
            new { Id = id },
            splitOn: "Role_RoleId"
        );

        return users.FirstOrDefault();
    }

    public async Task<int> AddAsync(User entity)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                        INSERT INTO Users 
                            (Login, PasswordHash, FullName, Email, Phone, RoleId, IsActive, CreatedDate)
                        VALUES 
                            (@Login, @PasswordHash, @FullName, @Email, @Phone, @RoleId, @IsActive, @CreatedDate);
                        SELECT CAST(SCOPE_IDENTITY() as int)
            """;

        return await connection.QuerySingleAsync<int>(sql, entity);
    }

    public async Task<bool> UpdateAsync(User entity)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                        UPDATE Users 
                        SET Login = @Login,
                            PasswordHash = @PasswordHash,
                            FullName = @FullName,
                            Email = @Email,
                            Phone = @Phone,
                            RoleId = @RoleId,
                            IsActive = @IsActive
                        WHERE UserId = @UserId
            """;

        var affectedRows = await connection.ExecuteAsync(sql, entity);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Users WHERE UserId = @Id";

        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                        SELECT u.*, r.RoleId AS Role_RoleId, r.*
                        FROM Users u
                        LEFT JOIN Roles r ON u.RoleId = r.RoleId
                        WHERE u.Login = @Login
            """;

        var users = await connection.QueryAsync<User, Role, User>(
            sql,
            (user, role) =>
            {
                user.Role = role;
                return user;
            },
            new { Login = login },
            splitOn: "Role_RoleId"
        );

        return users.FirstOrDefault();
    }
}
