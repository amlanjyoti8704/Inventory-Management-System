// using MySql.Data.MySqlClient;
using MySqlConnector;

public class DbContext
{
    private string connectionString = "server=localhost;user=root;password=7295883411;database=IT_consumables";
    // private string connectionString = "server=localhost;user=amlan;password=test1234;database=IT_consumables;";

    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
    }
}