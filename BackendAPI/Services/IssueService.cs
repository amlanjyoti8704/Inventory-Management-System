using BackendAPI.Data;
using BackendAPI.Models;
using Npgsql;

namespace BackendAPI.Services
{
    /// <summary>
    /// Manages the full lifecycle of item issue requests:
    /// create → approve/decline → return request → approve/reject return.
    /// 
    /// Stock adjustments happen at two points:
    /// - Approval: stock is DEDUCTED
    /// - Return approval: stock is RESTORED
    /// </summary>
    public class IssueService
    {
        private readonly DbHelper _db;

        public IssueService(DbHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns a simplified item list (id, name, stock) for the issue form dropdown.
        /// </summary>
        public async Task<List<object>> GetItemsForIssueAsync()
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT item_id, name, quantity FROM consumable_items ORDER BY item_id", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            var items = new List<object>();
            while (await reader.ReadAsync())
            {
                items.Add(new
                {
                    item_id = reader.GetInt32(0),
                    item_name = reader.GetString(1),
                    stock = reader.GetInt32(2)
                });
            }
            return items;
        }

        /// <summary>
        /// Creates a new issue request with status="pending".
        /// No stock is deducted here — that happens on approval.
        /// </summary>
        public async Task CreateRequestAsync(IssueRequest request)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO issue_records
                    (issued_to, department, quantity, item_id, issue_date, status, requested_by, return_status)
                VALUES
                    (@to, @dept, @qty, @itemId, @date, 'pending', @by, 'none')", conn);

            cmd.Parameters.AddWithValue("to", request.IssuedTo);
            cmd.Parameters.AddWithValue("dept", request.Department);
            cmd.Parameters.AddWithValue("qty", request.Quantity);
            cmd.Parameters.AddWithValue("itemId", request.ItemId);
            cmd.Parameters.AddWithValue("date", DateTime.Now);
            cmd.Parameters.AddWithValue("by", request.RequestedBy ?? "user");

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Returns all issue records, optionally filtered by requested_by.
        /// JOINs with consumable_items to include item_name in the response.
        /// </summary>
        public async Task<List<object>> GetAllAsync(string? requestedBy)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT ir.issue_id, ci.name AS item_name, ir.issued_to, ir.department,
                       ir.quantity, ir.issue_date, ir.status, ir.requested_by, ir.return_status
                FROM issue_records ir
                LEFT JOIN consumable_items ci ON ir.item_id = ci.item_id";

            if (!string.IsNullOrEmpty(requestedBy))
                sql += " WHERE ir.requested_by = @by";

            sql += " ORDER BY ir.issue_id";

            using var cmd = new NpgsqlCommand(sql, conn);
            if (!string.IsNullOrEmpty(requestedBy))
                cmd.Parameters.AddWithValue("by", requestedBy);

            using var reader = await cmd.ExecuteReaderAsync();
            var results = new List<object>();
            while (await reader.ReadAsync())
            {
                results.Add(new
                {
                    issue_id = reader.GetInt32(0),
                    item_name = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1),
                    issued_to = reader.GetString(2),
                    department = reader.GetString(3),
                    quantity = reader.GetInt32(4),
                    issue_date = reader.GetDateTime(5),
                    status = reader.GetString(6),
                    requested_by = reader.GetString(7),
                    return_status = reader.GetString(8)
                });
            }
            return results;
        }

        /// <summary>
        /// Sets status to "declined". No stock changes.
        /// Only works if the issue record exists.
        /// </summary>
        public async Task<bool> DeclineAsync(int issueId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "UPDATE issue_records SET status = 'declined' WHERE issue_id = @id", conn);
            cmd.Parameters.AddWithValue("id", issueId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        /// <summary>
        /// Approves an issue request. This is a multi-step operation:
        /// 1. Find the pending issue to get item_id and quantity
        /// 2. Check the item has enough stock
        /// 3. Deduct stock from consumable_items
        /// 4. Update issue status to "approved"
        /// 
        /// Returns (success, errorMessage).
        /// </summary>
        public async Task<(bool success, string? error)> ApproveAsync(int issueId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // Step 1: Find the pending issue
            using var findCmd = new NpgsqlCommand(@"
                SELECT item_id, quantity FROM issue_records
                WHERE issue_id = @id AND status = 'pending'", conn);
            findCmd.Parameters.AddWithValue("id", issueId);

            using var reader = await findCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return (false, "Request not found or already processed.");

            int itemId = reader.GetInt32(0);
            int issueQty = reader.GetInt32(1);
            await reader.CloseAsync();

            // Step 2: Check stock availability
            using var stockCmd = new NpgsqlCommand(
                "SELECT quantity FROM consumable_items WHERE item_id = @id", conn);
            stockCmd.Parameters.AddWithValue("id", itemId);

            var currentStock = await stockCmd.ExecuteScalarAsync();
            if (currentStock == null || (int)currentStock < issueQty)
                return (false, "Not enough stock.");

            // Step 3: Deduct stock
            using var deductCmd = new NpgsqlCommand(
                "UPDATE consumable_items SET quantity = quantity - @qty WHERE item_id = @id", conn);
            deductCmd.Parameters.AddWithValue("qty", issueQty);
            deductCmd.Parameters.AddWithValue("id", itemId);
            await deductCmd.ExecuteNonQueryAsync();

            // Step 4: Mark as approved
            using var approveCmd = new NpgsqlCommand(
                "UPDATE issue_records SET status = 'approved' WHERE issue_id = @id", conn);
            approveCmd.Parameters.AddWithValue("id", issueId);
            await approveCmd.ExecuteNonQueryAsync();

            return (true, null);
        }

        /// <summary>
        /// Deletes an issue record entirely. Returns false if not found.
        /// </summary>
        public async Task<bool> DeleteAsync(int issueId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "DELETE FROM issue_records WHERE issue_id = @id", conn);
            cmd.Parameters.AddWithValue("id", issueId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        /// <summary>
        /// User requests a return for an approved issue.
        /// Only works if status="approved" and return_status="none".
        /// Sets return_status to "requested".
        /// </summary>
        public async Task<bool> RequestReturnAsync(int issueId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                UPDATE issue_records SET return_status = 'requested'
                WHERE issue_id = @id AND status = 'approved' AND return_status = 'none'", conn);
            cmd.Parameters.AddWithValue("id", issueId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        /// <summary>
        /// Admin approves a return request. This restores stock:
        /// 1. Find the issue (must be approved + return requested)
        /// 2. Add the issued quantity back to consumable_items
        /// 3. Set return_status to "approved"
        /// </summary>
        public async Task<(bool success, string? error)> ApproveReturnAsync(int issueId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // Find the issue that's approved with a return request
            using var findCmd = new NpgsqlCommand(@"
                SELECT item_id, quantity FROM issue_records
                WHERE issue_id = @id AND status = 'approved' AND return_status = 'requested'", conn);
            findCmd.Parameters.AddWithValue("id", issueId);

            using var reader = await findCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return (false, "Record not found or already returned.");

            int itemId = reader.GetInt32(0);
            int qty = reader.GetInt32(1);
            await reader.CloseAsync();

            // Restore stock
            using var stockCmd = new NpgsqlCommand(
                "UPDATE consumable_items SET quantity = quantity + @qty WHERE item_id = @id", conn);
            stockCmd.Parameters.AddWithValue("qty", qty);
            stockCmd.Parameters.AddWithValue("id", itemId);
            await stockCmd.ExecuteNonQueryAsync();

            // Mark return as approved
            using var updateCmd = new NpgsqlCommand(
                "UPDATE issue_records SET return_status = 'approved' WHERE issue_id = @id", conn);
            updateCmd.Parameters.AddWithValue("id", issueId);
            await updateCmd.ExecuteNonQueryAsync();

            return (true, null);
        }

        /// <summary>
        /// Admin rejects a return request. No stock changes.
        /// Only works if return_status="requested".
        /// </summary>
        public async Task<bool> RejectReturnAsync(int issueId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                UPDATE issue_records SET return_status = 'rejected'
                WHERE issue_id = @id AND return_status = 'requested'", conn);
            cmd.Parameters.AddWithValue("id", issueId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        /// <summary>
        /// Returns all issues that need admin attention:
        /// - Pending approval (status IN ('pending','requested'))
        /// - Pending return approval (return_status = 'requested')
        /// </summary>
        public async Task<List<object>> GetPendingRequestsAsync()
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                SELECT issue_id, issued_to, department, quantity, item_id,
                       issue_date, status, requested_by, return_status
                FROM issue_records
                WHERE status IN ('pending', 'requested') OR return_status = 'requested'
                ORDER BY issue_id", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            var results = new List<object>();
            while (await reader.ReadAsync())
            {
                results.Add(new
                {
                    issueId = reader.GetInt32(0),
                    issuedTo = reader.GetString(1),
                    department = reader.GetString(2),
                    quantity = reader.GetInt32(3),
                    itemId = reader.GetInt32(4),
                    issueDate = reader.GetDateTime(5),
                    status = reader.GetString(6),
                    requestedBy = reader.GetString(7),
                    returnStatus = reader.GetString(8)
                });
            }
            return results;
        }
    }
}
