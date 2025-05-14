using Dapper;
using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Data;

public abstract class LostItemRepository(IDatabaseConnectionFactory connectionFactory) : IRepository<LostItem>
{
    public async Task<IEnumerable<LostItem>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       SELECT i.*, c.*, s.*
                                       FROM LostItems i
                                       LEFT JOIN Categories c ON i.CategoryId = c.CategoryId
                                       LEFT JOIN StorageLocations s ON i.StorageLocationId = s.StorageLocationId
                                       ORDER BY i.FoundDate DESC
                           """;
        
        var lostItemDictionary = new Dictionary<int, LostItem>();
        
        await connection.QueryAsync<LostItem, Category, StorageLocation, LostItem>(
            sql,
            (item, category, storageLocation) =>
            {
                if (!lostItemDictionary.TryGetValue(item.ItemId, out var existingItem))
                {
                    existingItem = item;
                    existingItem.Category = category;
                    existingItem.StorageLocation = storageLocation;
                    lostItemDictionary.Add(item.ItemId, existingItem);
                }
                
                return existingItem;
            },
            splitOn: "CategoryId,StorageLocationId");
        
        return lostItemDictionary.Values;
    }

    public async Task<LostItem?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       SELECT i.*, c.*, s.*, u.*
                                       FROM LostItems i
                                       LEFT JOIN Categories c ON i.CategoryId = c.CategoryId
                                       LEFT JOIN StorageLocations s ON i.StorageLocationId = s.StorageLocationId
                                       LEFT JOIN Users u ON i.RegisteredBy = u.UserId
                                       WHERE i.ItemId = @Id
                           """;
        
        var lostItems = await connection.QueryAsync<LostItem, Category, StorageLocation, User, LostItem>(
            sql,
            (item, category, storageLocation, user) =>
            {
                item.Category = category;
                item.StorageLocation = storageLocation;
                item.RegisteredByUser = user;
                return item;
            },
            new { Id = id },
            splitOn: "CategoryId,StorageLocationId,UserId");
        
        return lostItems.FirstOrDefault();
    }

    public async Task<int> AddAsync(LostItem entity)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       INSERT INTO LostItems 
                                           (ItemName, Description, FoundDate, FoundLocation, CategoryId, StorageLocationId, 
                                            Status, ImagePath, RegisteredBy, RegistrationDate)
                                       VALUES 
                                           (@ItemName, @Description, @FoundDate, @FoundLocation, @CategoryId, @StorageLocationId, 
                                            @Status, @ImagePath, @RegisteredBy, @RegistrationDate);
                                       SELECT CAST(SCOPE_IDENTITY() as int)
                           """;
        
        return await connection.QuerySingleAsync<int>(sql, entity);
    }

    public async Task<bool> UpdateAsync(LostItem entity)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       UPDATE LostItems 
                                       SET ItemName = @ItemName,
                                           Description = @Description,
                                           FoundDate = @FoundDate,
                                           FoundLocation = @FoundLocation,
                                           CategoryId = @CategoryId,
                                           StorageLocationId = @StorageLocationId,
                                           Status = @Status,
                                           ImagePath = @ImagePath
                                       WHERE ItemId = @ItemId
                           """;
        
        var affectedRows = await connection.ExecuteAsync(sql, entity);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM LostItems WHERE ItemId = @Id";
        
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }
    
    public async Task<IEnumerable<LostItem>> GetByStatusAsync(string status)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       SELECT i.*, c.*, s.*
                                       FROM LostItems i
                                       LEFT JOIN Categories c ON i.CategoryId = c.CategoryId
                                       LEFT JOIN StorageLocations s ON i.StorageLocationId = s.StorageLocationId
                                       WHERE i.Status = @Status
                                       ORDER BY i.FoundDate DESC
                           """;
        
        var lostItemDictionary = new Dictionary<int, LostItem>();
        
        await connection.QueryAsync<LostItem, Category, StorageLocation, LostItem>(
            sql,
            (item, category, storageLocation) =>
            {
                if (!lostItemDictionary.TryGetValue(item.ItemId, out var existingItem))
                {
                    existingItem = item;
                    existingItem.Category = category;
                    existingItem.StorageLocation = storageLocation;
                    lostItemDictionary.Add(item.ItemId, existingItem);
                }
                
                return existingItem;
            },
            new { Status = status },
            splitOn: "CategoryId,StorageLocationId");
        
        return lostItemDictionary.Values;
    }
    
    public async Task<IEnumerable<LostItem>> GetByCategoryAsync(int categoryId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       SELECT i.*, c.*, s.*
                                       FROM LostItems i
                                       LEFT JOIN Categories c ON i.CategoryId = c.CategoryId
                                       LEFT JOIN StorageLocations s ON i.StorageLocationId = s.StorageLocationId
                                       WHERE i.CategoryId = @CategoryId
                                       ORDER BY i.FoundDate DESC
                           """;
        
        var lostItemDictionary = new Dictionary<int, LostItem>();
        
        await connection.QueryAsync<LostItem, Category, StorageLocation, LostItem>(
            sql,
            (item, category, storageLocation) =>
            {
                if (!lostItemDictionary.TryGetValue(item.ItemId, out var existingItem))
                {
                    existingItem = item;
                    existingItem.Category = category;
                    existingItem.StorageLocation = storageLocation;
                    lostItemDictionary.Add(item.ItemId, existingItem);
                }
                
                return existingItem;
            },
            new { CategoryId = categoryId },
            splitOn: "CategoryId,StorageLocationId");
        
        return lostItemDictionary.Values;
    }
    
    public async Task<IEnumerable<LostItem>> SearchAsync(string searchTerm)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       SELECT i.*, c.*, s.*
                                       FROM LostItems i
                                       LEFT JOIN Categories c ON i.CategoryId = c.CategoryId
                                       LEFT JOIN StorageLocations s ON i.StorageLocationId = s.StorageLocationId
                                       WHERE i.ItemName LIKE @SearchTerm OR i.Description LIKE @SearchTerm
                                       ORDER BY i.FoundDate DESC
                           """;
        
        var lostItemDictionary = new Dictionary<int, LostItem>();
        
        await connection.QueryAsync<LostItem, Category, StorageLocation, LostItem>(
            sql,
            (item, category, storageLocation) =>
            {
                if (!lostItemDictionary.TryGetValue(item.ItemId, out var existingItem))
                {
                    existingItem = item;
                    existingItem.Category = category;
                    existingItem.StorageLocation = storageLocation;
                    lostItemDictionary.Add(item.ItemId, existingItem);
                }
                
                return existingItem;
            },
            new { SearchTerm = $"%{searchTerm}%" },
            splitOn: "CategoryId,StorageLocationId");
        
        return lostItemDictionary.Values;
    }
} 