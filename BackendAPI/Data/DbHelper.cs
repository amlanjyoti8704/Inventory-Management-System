using Npgsql;

namespace BackendAPI.Data
{
    /// <summary>
    /// Lightweight database helper that holds the connection string
    /// and creates NpgsqlConnection instances on demand.
    /// Each service method opens/closes its own connection (no shared state).
    /// </summary>
    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Creates a new NpgsqlConnection. The caller is responsible for opening and disposing it.
        /// Usage: using var conn = _db.CreateConnection(); await conn.OpenAsync();
        /// </summary>
        public NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
