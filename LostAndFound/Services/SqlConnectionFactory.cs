using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LostAndFound.Services;

public interface IDatabaseConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlConnectionFactory(IOptions<AppSettings> settings) : IDatabaseConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(settings.Value.ConnectionString);
        connection.Open();
        return connection;
    }
} 