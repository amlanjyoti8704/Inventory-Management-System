// File: Controllers/IssueController.cs

using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

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

    // ✅ GET: /api/issue/items — returns available items
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

    // ✅ POST: /api/issue — issue an item
    [HttpPost]
    public IActionResult IssueItem([FromBody] IssueRequest request)
    {
        using var conn = GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // 1. Check available stock
            var stockCmd = new MySqlCommand("SELECT quantity FROM consumableItems WHERE item_id = @item_id", conn);
            stockCmd.Parameters.AddWithValue("@item_id", request.item_id);
            var stock = Convert.ToInt32(stockCmd.ExecuteScalar());

            if (stock < request.quantity)
                return BadRequest("Insufficient stock");

            // 2. Reduce stock
            var updateStockCmd = new MySqlCommand("UPDATE consumableItems SET quantity = quantity - @qty WHERE item_id = @item_id", conn);
            updateStockCmd.Parameters.AddWithValue("@qty", request.quantity);
            updateStockCmd.Parameters.AddWithValue("@item_id", request.item_id);
            updateStockCmd.ExecuteNonQuery();

            // 3. Insert into issue_records
            var insertIssueCmd = new MySqlCommand(@"INSERT INTO issue_records (issued_to, department, quantity)
                                                    VALUES (@issued_to, @dept, @qty); SELECT LAST_INSERT_ID();", conn);
            insertIssueCmd.Parameters.AddWithValue("@issued_to", request.issued_to);
            insertIssueCmd.Parameters.AddWithValue("@dept", request.department);
            insertIssueCmd.Parameters.AddWithValue("@qty", request.quantity);
            var issue_id = Convert.ToInt32(insertIssueCmd.ExecuteScalar());

            // 4. Insert into issued table
            var insertIssuedCmd = new MySqlCommand("INSERT INTO issued (issue_id, item_id, issue_date) VALUES (@issue_id, @item_id, NOW())", conn);
            insertIssuedCmd.Parameters.AddWithValue("@issue_id", issue_id);
            insertIssuedCmd.Parameters.AddWithValue("@item_id", request.item_id);
            insertIssuedCmd.ExecuteNonQuery();

            transaction.Commit();
            return Ok(new { message = "Item issued successfully" });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ✅ GET: /api/issue — list of all issued items
    [HttpGet]
    public IActionResult GetIssuedItems()
    {
        var issues = new List<object>();
        using var conn = GetConnection();
        conn.Open();
        var cmd = new MySqlCommand(@"
            SELECT ir.issue_id, ci.name AS item_name, ir.issued_to, ir.department, ir.quantity, i.issue_date
            FROM issue_records ir
            JOIN issued i ON ir.issue_id = i.issue_id
            JOIN consumableItems ci ON i.item_id = ci.item_id", conn);
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
                issue_date = reader["issue_date"]
            });
        }
        return Ok(issues);
    }

    // ✅ DELETE: /api/issue/{id} — revoke an issue and restore stock
    [HttpDelete("{id}")]
    public IActionResult RevokeIssue(int id)
    {
        using var conn = GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Get item_id and quantity
            var getCmd = new MySqlCommand(@"
                SELECT i.item_id, ir.quantity 
                FROM issued i
                JOIN issue_records ir ON i.issue_id = ir.issue_id
                WHERE i.issue_id = @id", conn);
            getCmd.Parameters.AddWithValue("@id", id);
            using var reader = getCmd.ExecuteReader();
            if (!reader.Read()) return NotFound(new { message = "Issue ID not found" });

            int itemId = Convert.ToInt32(reader["item_id"]);
            int qty = Convert.ToInt32(reader["quantity"]);
            reader.Close();

            // 1. Delete from issued
            var delIssued = new MySqlCommand("DELETE FROM issued WHERE issue_id = @id", conn);
            delIssued.Parameters.AddWithValue("@id", id);
            delIssued.ExecuteNonQuery();

            // 2. Delete from issue_records
            var delRecords = new MySqlCommand("DELETE FROM issue_records WHERE issue_id = @id", conn);
            delRecords.Parameters.AddWithValue("@id", id);
            delRecords.ExecuteNonQuery();

            // 3. Restore stock
            var restoreCmd = new MySqlCommand("UPDATE consumableItems SET quantity = quantity + @qty WHERE item_id = @item_id", conn);
            restoreCmd.Parameters.AddWithValue("@qty", qty);
            restoreCmd.Parameters.AddWithValue("@item_id", itemId);
            restoreCmd.ExecuteNonQuery();

            transaction.Commit();
            return Ok(new { message = "Issue revoked and stock restored" });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// ✅ Request model for issuing
public class IssueRequest
{
    public int item_id { get; set; }
    public string issued_to { get; set; }
    public string department { get; set; }
    public int quantity { get; set; }
}