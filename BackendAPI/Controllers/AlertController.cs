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
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var alerts = new List<dynamic>();

        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        var query = @"
            SELECT a.log_id, a.item_id, i.name, a.current_quantity, a.alert_message, a.alert_time
            FROM alert_log a
            JOIN consumableItems i ON a.item_id = i.item_id
            WHERE 1=1";

        if (item_id.HasValue) query += " AND a.item_id = @item_id";
        if (startDate.HasValue) query += " AND a.alert_time >= @startDate";
        if (endDate.HasValue) query += " AND a.alert_time <= @endDate";

        var cmd = new MySqlCommand(query, connection);

        if (item_id.HasValue) cmd.Parameters.AddWithValue("@item_id", item_id.Value);
        if (startDate.HasValue) cmd.Parameters.AddWithValue("@startDate", startDate.Value);
        if (endDate.HasValue) cmd.Parameters.AddWithValue("@endDate", endDate.Value);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            alerts.Add(new
            {
                log_id = reader["log_id"],
                item_id = reader["item_id"],
                name = reader["name"],
                current_quantity = reader["current_quantity"],
                alert_message = reader["alert_message"],
                alert_time = ((DateTime)reader["alert_time"]).ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        return Ok(alerts);
    }
}