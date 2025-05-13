using System.Data;
using Microsoft.Data.SqlClient;

namespace LostAndFound.Services;

public interface IDatabaseConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlConnectionFactory(AppSettings settings) : IDatabaseConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(settings.ConnectionString);
        connection.Open();
        return connection;
    }
} 