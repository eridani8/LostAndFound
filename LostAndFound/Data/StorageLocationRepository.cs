using Dapper;
using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Data;

public class StorageLocationRepository(IDatabaseConnectionFactory connectionFactory)
    : IRepository<StorageLocation>
{
    public async Task<IEnumerable<StorageLocation>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM StorageLocations ORDER BY StorageLocationId DESC";

        return await connection.QueryAsync<StorageLocation>(sql);
    }

    public async Task<StorageLocation?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM StorageLocations WHERE StorageLocationId = @Id";

        return await connection.QueryFirstOrDefaultAsync<StorageLocation>(sql, new { Id = id });
    }

    public async Task<int> AddAsync(StorageLocation entity)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO StorageLocations (LocationName, Address, ContactPhone, WorkingHours)
            VALUES (@LocationName, @Address, @ContactPhone, @WorkingHours);
            SELECT CAST(SCOPE_IDENTITY() as int)
            """;

        return await connection.QuerySingleAsync<int>(sql, entity);
    }

    public async Task<bool> UpdateAsync(StorageLocation entity)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
            UPDATE StorageLocations 
            SET LocationName = @LocationName,
                Address = @Address,
                ContactPhone = @ContactPhone,
                WorkingHours = @WorkingHours
            WHERE StorageLocationId = @StorageLocationId
            """;

        var affectedRows = await connection.ExecuteAsync(sql, entity);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM StorageLocations WHERE StorageLocationId = @Id";

        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }
}
