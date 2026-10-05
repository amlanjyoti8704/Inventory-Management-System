using BackendAPI.Data;
using Npgsql;

namespace BackendAPI.Services
{
    /// <summary>
    /// Retrieves alert logs with optional filtering by item, category, and date range.
    /// Alert logs are created by database triggers (or background jobs) when stock drops
    /// below category thresholds.
    /// </summary>
    public class AlertService
    {
        private readonly DbHelper _db;

        public AlertService(DbHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// Fetches alert logs with optional filters.
        /// JOINs with consumable_items to include the item name in results.
        /// Filters are dynamically appended only when provided (AND-combined).
        /// Results are sorted newest-first.
        /// </summary>
        public async Task<List<object>> GetAlertsAsync(int? itemId, int? categoryId, DateTime? startDate, DateTime? endDate)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            var conditions = new List<string>();
            var cmd = new NpgsqlCommand();

            if (itemId.HasValue)
            {
                conditions.Add("al.item_id = @itemId");
                cmd.Parameters.AddWithValue("itemId", itemId.Value);
            }
            if (categoryId.HasValue)
            {
                conditions.Add("al.category_id = @categoryId");
                cmd.Parameters.AddWithValue("categoryId", categoryId.Value);
            }
            if (startDate.HasValue)
            {
                conditions.Add("al.alert_time >= @startDate");
                cmd.Parameters.AddWithValue("startDate", startDate.Value);
            }
            if (endDate.HasValue)
            {
                conditions.Add("al.alert_time <= @endDate");
                cmd.Parameters.AddWithValue("endDate", endDate.Value);
            }

            var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            // LEFT JOIN with consumable_items to get the item name
            cmd.CommandText = $@"
                SELECT al.log_id, al.item_id, al.category_id, ci.name,
                       al.category_name, al.current_quantity, al.alert_message, al.alert_time
                FROM alert_log al
                LEFT JOIN consumable_items ci ON al.item_id = ci.item_id
                {where}
                ORDER BY al.alert_time DESC";
            cmd.Connection = conn;

            using var reader = await cmd.ExecuteReaderAsync();
            var alerts = new List<object>();
            while (await reader.ReadAsync())
            {
                alerts.Add(new
                {
                    log_id = reader.GetInt32(0).ToString(),
                    item_id = reader.IsDBNull(1) ? "N/A" : reader.GetInt32(1).ToString(),
                    category_id = reader.IsDBNull(2) ? "N/A" : reader.GetInt32(2).ToString(),
                    name = reader.IsDBNull(3) ? "N/A" : reader.GetString(3),
                    category_name = reader.IsDBNull(4) ? "N/A" : reader.GetString(4),
                    current_quantity = reader.GetInt32(5).ToString(),
                    alert_message = reader.IsDBNull(6) ? "N/A" : reader.GetString(6),
                    alert_time = reader.GetDateTime(7).ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            return alerts;
        }
    }
}
