using Dapper;
using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Data;

public class ActionLogRepository(IDatabaseConnectionFactory connectionFactory)
    : IRepository<ActionLog>
{
    public async Task<IEnumerable<ActionLog>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                        SELECT l.*, u.UserId AS User_UserId, u.*
                        FROM ActionLog l
                        LEFT JOIN Users u ON l.UserId = u.UserId
                        ORDER BY l.ActionDate DESC
            """;

        var logDictionary = new Dictionary<int, ActionLog>();

        await connection.QueryAsync<ActionLog, User, ActionLog>(
            sql,
            (log, user) =>
            {
                if (!logDictionary.TryGetValue(log.LogId, out var existingLog))
                {
                    existingLog = log;
                    existingLog.User = user;
                    logDictionary.Add(log.LogId, existingLog);
                }

                return existingLog;
            },
            splitOn: "User_UserId"
        );

        return logDictionary.Values;
    }

    public async Task<ActionLog?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                        SELECT l.*, u.UserId AS User_UserId, u.*
                        FROM ActionLog l
                        LEFT JOIN Users u ON l.UserId = u.UserId
                        WHERE l.LogId = @Id
            """;

        var logs = await connection.QueryAsync<ActionLog, User, ActionLog>(
            sql,
            (log, user) =>
            {
                log.User = user;
                return log;
            },
            new { Id = id },
            splitOn: "User_UserId"
        );

        return logs.FirstOrDefault();
    }

    public async Task<int> AddAsync(ActionLog entity)
    {
        using var connection = connectionFactory.CreateConnection();

        if (entity.ActionDate == DateTime.MinValue)
        {
            entity.ActionDate = DateTime.Now;
        }

        const string sql = """

                        INSERT INTO ActionLog 
                            (UserId, ActionDate, ActionType, Details, IpAddress)
                        VALUES 
                            (@UserId, @ActionDate, @ActionType, @Details, @IpAddress);
                        SELECT CAST(SCOPE_IDENTITY() as int)
            """;

        return await connection.QuerySingleAsync<int>(sql, entity);
    }

    public async Task<bool> UpdateAsync(ActionLog entity)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                        UPDATE ActionLog 
                        SET UserId = @UserId,
                            ActionType = @ActionType,
                            Description = @Description,
                            Timestamp = @Timestamp,
                            IpAddress = @IpAddress
                        WHERE LogId = @LogId
            """;

        var affectedRows = await connection.ExecuteAsync(sql, entity);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM ActionLog WHERE LogId = @Id";

        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }

    public async Task<IEnumerable<ActionLog>> GetByUserIdAsync(int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                        SELECT l.*, u.UserId AS User_UserId, u.*
                        FROM ActionLog l
                        LEFT JOIN Users u ON l.UserId = u.UserId
                        WHERE l.UserId = @UserId
                        ORDER BY l.ActionDate DESC
            """;

        var logDictionary = new Dictionary<int, ActionLog>();

        await connection.QueryAsync<ActionLog, User, ActionLog>(
            sql,
            (log, user) =>
            {
                if (!logDictionary.TryGetValue(log.LogId, out var existingLog))
                {
                    existingLog = log;
                    existingLog.User = user;
                    logDictionary.Add(log.LogId, existingLog);
                }

                return existingLog;
            },
            new { UserId = userId },
            splitOn: "User_UserId"
        );

        return logDictionary.Values;
    }

    public async Task<IEnumerable<ActionLog>> GetByActionTypeAsync(string actionType)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                        SELECT l.*, u.UserId AS User_UserId, u.*
                        FROM ActionLog l
                        LEFT JOIN Users u ON l.UserId = u.UserId
                        WHERE l.ActionType = @ActionType
                        ORDER BY l.ActionDate DESC
            """;

        var logDictionary = new Dictionary<int, ActionLog>();

        await connection.QueryAsync<ActionLog, User, ActionLog>(
            sql,
            (log, user) =>
            {
                if (!logDictionary.TryGetValue(log.LogId, out var existingLog))
                {
                    existingLog = log;
                    existingLog.User = user;
                    logDictionary.Add(log.LogId, existingLog);
                }

                return existingLog;
            },
            new { ActionType = actionType },
            splitOn: "User_UserId"
        );

        return logDictionary.Values;
    }

    public async Task<IEnumerable<ActionLog>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                        SELECT l.*, u.UserId AS User_UserId, u.*
                        FROM ActionLog l
                        LEFT JOIN Users u ON l.UserId = u.UserId
                        WHERE l.ActionDate BETWEEN @StartDate AND @EndDate
                        ORDER BY l.ActionDate DESC
            """;

        var logDictionary = new Dictionary<int, ActionLog>();

        await connection.QueryAsync<ActionLog, User, ActionLog>(
            sql,
            (log, user) =>
            {
                if (!logDictionary.TryGetValue(log.LogId, out var existingLog))
                {
                    existingLog = log;
                    existingLog.User = user;
                    logDictionary.Add(log.LogId, existingLog);
                }

                return existingLog;
            },
            new { StartDate = startDate, EndDate = endDate },
            splitOn: "User_UserId"
        );

        return logDictionary.Values;
    }
}
