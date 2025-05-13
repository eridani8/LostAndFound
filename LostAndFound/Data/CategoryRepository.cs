using Dapper;
using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Data;

public class CategoryRepository(IDatabaseConnectionFactory connectionFactory) : IRepository<Category>
{
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryAsync<Category>("SELECT * FROM Categories ORDER BY CategoryId DESC");
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Category>(
            "SELECT * FROM Categories WHERE CategoryId = @Id", 
            new { Id = id });
    }

    public async Task<int> AddAsync(Category entity)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       INSERT INTO Categories (CategoryName, Description)
                                       VALUES (@CategoryName, @Description);
                                       SELECT CAST(SCOPE_IDENTITY() as int)
                           """;
        
        return await connection.QuerySingleAsync<int>(sql, entity);
    }

    public async Task<bool> UpdateAsync(Category entity)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       UPDATE Categories 
                                       SET CategoryName = @CategoryName,
                                           Description = @Description
                                       WHERE CategoryId = @CategoryId
                           """;
        
        var affectedRows = await connection.ExecuteAsync(sql, entity);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Categories WHERE CategoryId = @Id";
        
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }
} 