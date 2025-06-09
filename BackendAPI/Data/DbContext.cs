using Microsoft.Extensions.Configuration;
using MySqlConnector;
using BackendAPI.Models; // Or wherever your IssueRecord.cs is located
public class DbContext
{
    private readonly string _connectionString;

    public DbContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(_connectionString);
    }
    public void ExecuteQuery(string query, Dictionary<string, object> parameters)
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand(query, conn);
        foreach (var param in parameters)
        {
            cmd.Parameters.AddWithValue(param.Key, param.Value);
        }
        cmd.ExecuteNonQuery();
    }
}