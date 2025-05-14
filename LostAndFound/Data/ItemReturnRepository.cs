using System.Data;
using Dapper;
using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Data;

public class ItemReturnRepository(IDatabaseConnectionFactory connectionFactory) : IRepository<ItemReturn>
{
    public async Task<IEnumerable<ItemReturn>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       SELECT r.*, i.*, u.*
                                       FROM ItemReturns r
                                       LEFT JOIN LostItems i ON r.ItemId = i.ItemId
                                       LEFT JOIN Users u ON r.ReceivedBy = u.UserId
                                       ORDER BY r.ReturnId DESC
                           """;

        var returnDictionary = new Dictionary<int, ItemReturn>();

        await connection.QueryAsync<ItemReturn, LostItem, User, ItemReturn>(
            sql,
            (itemReturn, lostItem, user) =>
            {
                if (!returnDictionary.TryGetValue(itemReturn.ReturnId, out var existingReturn))
                {
                    existingReturn = itemReturn;
                    existingReturn.LostItem = lostItem;
                    existingReturn.ReceivedByUser = user;
                    returnDictionary.Add(itemReturn.ReturnId, existingReturn);
                }

                return existingReturn;
            },
            splitOn: "ItemId,UserId");

        return returnDictionary.Values;
    }

    public async Task<ItemReturn?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       SELECT r.*, i.*, u.*
                                       FROM ItemReturns r
                                       LEFT JOIN LostItems i ON r.ItemId = i.ItemId
                                       LEFT JOIN Users u ON r.ReceivedBy = u.UserId
                                       WHERE r.ReturnId = @Id
                           """;

        var returns = await connection.QueryAsync<ItemReturn, LostItem, User, ItemReturn>(
            sql,
            (itemReturn, lostItem, user) =>
            {
                itemReturn.LostItem = lostItem;
                itemReturn.ReceivedByUser = user;
                return itemReturn;
            },
            new { Id = id },
            splitOn: "ItemId,UserId");

        return returns.FirstOrDefault();
    }

    public async Task<int> AddAsync(ItemReturn entity)
    {
        using var connection = connectionFactory.CreateConnection();

        const string updateItemSql = """

                                                 UPDATE LostItems 
                                                 SET Status = N'Returned'
                                                 WHERE ItemId = @ItemId
                                     """;

        await connection.ExecuteAsync(updateItemSql, new { entity.ItemId });

        const string sql = """

                                       INSERT INTO ItemReturns 
                                           (ItemId, ReturnedTo, ReturnDate, ContactInfo, ReceivedBy, Notes)
                                       VALUES 
                                           (@ItemId, @ReturnedTo, @ReturnDate, @ContactInfo, @ReceivedBy, @Notes);
                                       SELECT CAST(SCOPE_IDENTITY() as int)
                           """;

        return await connection.QuerySingleAsync<int>(sql, entity);
    }

    public async Task<bool> UpdateAsync(ItemReturn entity)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       UPDATE ItemReturns 
                                       SET ItemId = @ItemId,
                                           ReturnedTo = @ReturnedTo,
                                           ReturnDate = @ReturnDate,
                                           ContactInfo = @ContactInfo,
                                           ReceivedBy = @ReceivedBy,
                                           Notes = @Notes
                                       WHERE ReturnId = @ReturnId
                           """;

        var affectedRows = await connection.ExecuteAsync(sql, entity);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();

        const string getItemIdSql = "SELECT ItemId FROM ItemReturns WHERE ReturnId = @Id";
        var itemId = await connection.QuerySingleOrDefaultAsync<int?>(getItemIdSql, new { Id = id });

        if (itemId.HasValue)
        {
            const string updateItemSql = """

                                                         UPDATE LostItems 
                                                         SET Status = N'Waiting'
                                                         WHERE ItemId = @ItemId
                                         """;

            await connection.ExecuteAsync(updateItemSql, new { ItemId = itemId.Value });
        }

        const string sql = "DELETE FROM ItemReturns WHERE ReturnId = @Id";
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });

        return affectedRows > 0;
    }

    public async Task<IEnumerable<ItemReturn>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """

                                       SELECT r.*, i.*, u.*
                                       FROM ItemReturns r
                                       LEFT JOIN LostItems i ON r.ItemId = i.ItemId
                                       LEFT JOIN Users u ON r.ReceivedBy = u.UserId
                                       WHERE r.ReturnDate BETWEEN @StartDate AND @EndDate
                                       ORDER BY r.ReturnDate DESC
                           """;

        var returnDictionary = new Dictionary<int, ItemReturn>();

        await connection.QueryAsync<ItemReturn, LostItem, User, ItemReturn>(
            sql,
            (itemReturn, lostItem, user) =>
            {
                if (!returnDictionary.TryGetValue(itemReturn.ReturnId, out var existingReturn))
                {
                    existingReturn = itemReturn;
                    existingReturn.LostItem = lostItem;
                    existingReturn.ReceivedByUser = user;
                    returnDictionary.Add(itemReturn.ReturnId, existingReturn);
                }

                return existingReturn;
            },
            new { StartDate = startDate, EndDate = endDate },
            splitOn: "ItemId,UserId");

        return returnDictionary.Values;
    }
}