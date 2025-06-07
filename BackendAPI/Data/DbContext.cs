// using MySql.Data.MySqlClient;
using MySqlConnector;
using BackendAPI.Models; // Or wherever your IssueRecord.cs is located
public class DbContext
{
    private string connectionString = "server=localhost;user=root;password=7295883411;database=IT_consumables";
    // private string connectionString = "server=localhost;user=amlan;password=test1234;database=IT_consumables;";

    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
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