using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using System.Collections.Generic;

[Route("api/[controller]")]
[ApiController]
public class AlertController : ControllerBase
{
    private readonly string _connectionString;

    public AlertController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [HttpGet]
    public IActionResult GetAlerts(
        [FromQuery] int? item_id,
        [FromQuery] int? category_id,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var alerts = new List<dynamic>();

        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        var conditions = new List<string>();
        if (item_id.HasValue) conditions.Add("a.item_id = @item_id");
        if (category_id.HasValue) conditions.Add("a.category_id = @category_id");
        if (startDate.HasValue) conditions.Add("a.alert_time >= @startDate");
        if (endDate.HasValue) conditions.Add("a.alert_time <= @endDate");

        var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

        var query = $@"
            SELECT 
                a.log_id,
                a.item_id,
                a.category_id,
                i.name,
                a.category_name,
                a.current_quantity,
                a.alert_message,
                a.alert_time
            FROM alert_log a
            LEFT JOIN consumableItems i ON a.item_id = i.item_id
            {whereClause}
            ORDER BY a.alert_time DESC
        ";

        var cmd = new MySqlCommand(query, connection);

        if (item_id.HasValue) cmd.Parameters.AddWithValue("@item_id", item_id.Value);
        if (category_id.HasValue) cmd.Parameters.AddWithValue("@category_id", category_id.Value);
        if (startDate.HasValue) cmd.Parameters.AddWithValue("@startDate", startDate.Value);
        if (endDate.HasValue) cmd.Parameters.AddWithValue("@endDate", endDate.Value);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            alerts.Add(new
            {
                log_id = reader["log_id"] is DBNull ? "N/A" : reader["log_id"].ToString(),
                item_id = reader["item_id"] is DBNull ? "N/A" : reader["item_id"].ToString(),
                category_id = reader["category_id"] is DBNull ? "N/A" : reader["category_id"].ToString(),
                name = reader["name"] is DBNull ? "N/A" : reader["name"].ToString(),
                category_name = reader["category_name"] is DBNull ? "N/A" : reader["category_name"].ToString(),
                current_quantity = reader["current_quantity"] is DBNull ? "N/A" : reader["current_quantity"].ToString(),
                alert_message = reader["alert_message"] is DBNull ? "N/A" : reader["alert_message"].ToString(),
                alert_time = reader["alert_time"] is DBNull
                    ? "N/A"
                    : Convert.ToDateTime(reader["alert_time"]).ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        return Ok(alerts);
    }
}
