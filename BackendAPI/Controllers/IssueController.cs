using Microsoft.AspNetCore.Mvc;
// using MySql.Data.MySqlClient;
using MySqlConnector;
using System.Data;
using System.Collections.Generic;

[Route("api/[controller]")]
[ApiController]
public class IssueController : ControllerBase
{
    private readonly IConfiguration _config;

    public IssueController(IConfiguration config)
    {
        _config = config;
    }

    private MySqlConnection GetConnection()
    {
        return new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
    }

    [HttpGet("items")]
    public IActionResult GetItems()
    {
        var items = new List<object>();
        using var conn = GetConnection();
        conn.Open();
        var cmd = new MySqlCommand("SELECT item_id, name AS item_name, quantity AS stock FROM consumableItems", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new
            {
                item_id = reader["item_id"],
                item_name = reader["item_name"],
                stock = reader["stock"]
            });
        }
        return Ok(items);
    }

    // POST: Create a new issue request (status = 'pending'), no stock change here!
    [HttpPost]
    public IActionResult CreateIssueRequest([FromBody] IssueRequest request)
    {
        using var conn = GetConnection();
        conn.Open();
        try
        {
            var insertCmd = new MySqlCommand(@"
                INSERT INTO issue_records 
                (issued_to, department, quantity, item_id, issue_date, status, requested_by) 
                VALUES (@issued_to, @department, @quantity, @item_id, NOW(), 'pending', @requested_by);", conn);
            insertCmd.Parameters.AddWithValue("@issued_to", request.issued_to);
            insertCmd.Parameters.AddWithValue("@department", request.department);
            insertCmd.Parameters.AddWithValue("@quantity", request.quantity);
            insertCmd.Parameters.AddWithValue("@item_id", request.item_id);
            insertCmd.Parameters.AddWithValue("@requested_by", request.requested_by ?? "user");
            insertCmd.ExecuteNonQuery();

            return Ok(new { message = "Request submitted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetIssuedItems([FromQuery] string? requested_by)
    {
        var issues = new List<object>();
        using var conn = GetConnection();
        conn.Open();

        string query = @"
            SELECT ir.issue_id, ci.name AS item_name, ir.issued_to, ir.department, ir.quantity,
                   ir.issue_date, ir.status, ir.requested_by, ir.return_status
            FROM issue_records ir
            JOIN consumableItems ci ON ir.item_id = ci.item_id";

        if (!string.IsNullOrEmpty(requested_by))
            query += " WHERE ir.requested_by = @requested_by";

        using var cmd = new MySqlCommand(query, conn);
        if (!string.IsNullOrEmpty(requested_by))
            cmd.Parameters.AddWithValue("@requested_by", requested_by);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            issues.Add(new
            {
                issue_id = reader["issue_id"],
                item_name = reader["item_name"],
                issued_to = reader["issued_to"],
                department = reader["department"],
                quantity = reader["quantity"],
                issue_date = reader["issue_date"],
                status = reader["status"],
                requested_by = reader["requested_by"],
                return_status = reader["return_status"]
            });
        }
        return Ok(issues);
    }

    [HttpPut("decline/{id}")]
    public IActionResult DeclineIssue(int id)
    {
        using var conn = GetConnection();
        conn.Open();
        try
        {
            var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM issue_records WHERE issue_id = @id", conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (!exists)
                return NotFound(new { error = "Request not found." });

            var updateStatus = new MySqlCommand("UPDATE issue_records SET status = 'declined' WHERE issue_id = @id", conn);
            updateStatus.Parameters.AddWithValue("@id", id);
            updateStatus.ExecuteNonQuery();

            return Ok(new { message = "Request declined." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // PUT: Approve request and deduct stock
    [HttpPut("approve/{id}")]
    public IActionResult ApproveIssue(int id)
    {
        using var conn = GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            var getCmd = new MySqlCommand("SELECT quantity, item_id FROM issue_records WHERE issue_id = @id AND status = 'pending'", conn);
            getCmd.Parameters.AddWithValue("@id", id);
            using var reader = getCmd.ExecuteReader();
            if (!reader.Read()) return NotFound(new { error = "Pending request not found." });

            int qty = Convert.ToInt32(reader["quantity"]);
            int itemId = Convert.ToInt32(reader["item_id"]);
            reader.Close();

            var stockCmd = new MySqlCommand("SELECT quantity FROM consumableItems WHERE item_id = @item_id", conn);
            stockCmd.Parameters.AddWithValue("@item_id", itemId);
            int stock = Convert.ToInt32(stockCmd.ExecuteScalar());
            if (stock < qty) return BadRequest(new { error = "Insufficient stock." });

            var updateStock = new MySqlCommand("UPDATE consumableItems SET quantity = quantity - @qty WHERE item_id = @item_id", conn);
            updateStock.Parameters.AddWithValue("@qty", qty);
            updateStock.Parameters.AddWithValue("@item_id", itemId);
            updateStock.ExecuteNonQuery();

            var updateStatus = new MySqlCommand("UPDATE issue_records SET status = 'approved', issue_date = NOW() WHERE issue_id = @id", conn);
            updateStatus.Parameters.AddWithValue("@id", id);
            updateStatus.ExecuteNonQuery();

            transaction.Commit();
            return Ok(new { message = "Request approved and issued." });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteIssueRecord(int id)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();

            string query = "DELETE FROM issue_records WHERE issue_id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return Ok(new { message = "Issue record deleted successfully." });
            }
            else
            {
                return NotFound(new { error = "Issue record not found." });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("request-return/{id}")]
    public IActionResult RequestReturn(int id)
    {
        using var conn = GetConnection();
        conn.Open();
        try
        {
            var updateCmd = new MySqlCommand("UPDATE issue_records SET return_status = 'requested' WHERE issue_id = @id AND status = 'approved'", conn);
            updateCmd.Parameters.AddWithValue("@id", id);
            int affected = updateCmd.ExecuteNonQuery();

            if (affected == 0)
                return BadRequest(new { error = "Return request failed. Either invalid ID or not in approved state." });

            return Ok(new { message = "Return request sent to admin." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("approve-return/{id}")]
    public IActionResult ApproveReturn(int id)
    {
        using var conn = GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Get issue details
            var selectCmd = new MySqlCommand("SELECT item_id, quantity FROM issue_records WHERE issue_id = @id AND return_status = 'requested'", conn);
            selectCmd.Parameters.AddWithValue("@id", id);
            using var reader = selectCmd.ExecuteReader();
            if (!reader.Read())
                return NotFound(new { error = "No matching return request found." });

            int itemId = Convert.ToInt32(reader["item_id"]);
            int quantity = Convert.ToInt32(reader["quantity"]);
            reader.Close();

            // Increase stock
            var updateStock = new MySqlCommand("UPDATE consumableItems SET quantity = quantity + @qty WHERE item_id = @item_id", conn, transaction);
            updateStock.Parameters.AddWithValue("@qty", quantity);
            updateStock.Parameters.AddWithValue("@item_id", itemId);
            updateStock.ExecuteNonQuery();

            // Update return status
            var updateReturn = new MySqlCommand("UPDATE issue_records SET return_status = 'approved', status = 'returned' WHERE issue_id = @id", conn, transaction);
            updateReturn.Parameters.AddWithValue("@id", id);
            updateReturn.ExecuteNonQuery();

            transaction.Commit();
            return Ok(new { message = "Return approved and stock updated." });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("reject-return/{id}")]
    public IActionResult RejectReturn(int id)
    {
        using var conn = GetConnection();
        conn.Open();
        try
        {
            var updateCmd = new MySqlCommand("UPDATE issue_records SET return_status = 'rejected' WHERE issue_id = @id AND return_status = 'requested'", conn);
            updateCmd.Parameters.AddWithValue("@id", id);
            int affected = updateCmd.ExecuteNonQuery();

            if (affected == 0)
                return BadRequest(new { error = "Reject failed. Invalid request or not in 'requested' state." });

            return Ok(new { message = "Return request rejected." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("pending-requests")]
    public IActionResult GetPendingRequests()
    {
        List<IssueRecords> pendingIssues = new List<IssueRecords>();

        var db = new DbContext();
        using var conn = db.GetConnection();
        conn.Open();

        string query = @"SELECT * FROM issue_records 
                WHERE status IN ('pending', 'requested') 
                OR LOWER(TRIM(return_status)) = 'requested'";

        using var cmd = new MySqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            pendingIssues.Add(new IssueRecords
            {
                IssueId = Convert.ToInt32(reader["issue_id"]),
                IssuedTo = reader["issued_to"].ToString(),
                Department = reader["department"].ToString(),
                Quantity = Convert.ToInt32(reader["quantity"]),
                requested_by = reader["requested_by"].ToString(),
                Status = reader["status"].ToString(),
                ReturnStatus = reader["return_status"] == DBNull.Value ? null : reader["return_status"].ToString(),                IssueDate = Convert.ToDateTime(reader["issue_date"])
            });
        }

        return Ok(pendingIssues);
    }


}

// ✅ Model
public class IssueRequest
{
    public int item_id { get; set; }
    public string issued_to { get; set; }
    public string department { get; set; }
    public int quantity { get; set; }
    public string requested_by { get; set; }
}