using BackendAPI.Data;
using Npgsql;

namespace BackendAPI.Services
{
    /// <summary>
    /// Provides aggregated dashboard statistics.
    /// All counts are fetched via SQL COUNT(*) for efficiency — no full table scans.
    /// Low stock detection uses a JOIN between items and categories to compare
    /// item quantity against category threshold.
    /// </summary>
    public class DashboardService
    {
        private readonly DbHelper _db;

        public DashboardService(DbHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns a summary object with all dashboard KPIs:
        /// - Total categories, items, purchase orders, issues
        /// - Low stock items (quantity below category threshold)
        /// - Pending issues awaiting admin approval
        /// - Return requests awaiting admin approval
        /// 
        /// Uses a single connection with multiple queries for efficiency.
        /// </summary>
        public async Task<object> GetSummaryAsync()
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // Count categories
            var totalCategories = (long)(await new NpgsqlCommand(
                "SELECT COUNT(*) FROM category", conn).ExecuteScalarAsync())!;

            // Count items
            var totalItems = (long)(await new NpgsqlCommand(
                "SELECT COUNT(*) FROM consumable_items", conn).ExecuteScalarAsync())!;

            // Low stock: items whose quantity is below their category's threshold.
            // This replaces the old MongoDB approach that loaded all items + categories
            // into memory and compared them in C# — the SQL JOIN is far more efficient.
            var lowStockItems = (long)(await new NpgsqlCommand(@"
                SELECT COUNT(*) FROM consumable_items ci
                JOIN category c ON ci.category_id = c.category_id
                WHERE ci.quantity < c.threshold", conn).ExecuteScalarAsync())!;

            // Count purchase orders
            var totalPurchaseOrders = (long)(await new NpgsqlCommand(
                "SELECT COUNT(*) FROM purchase_details", conn).ExecuteScalarAsync())!;

            // Count all issue records
            var totalIssues = (long)(await new NpgsqlCommand(
                "SELECT COUNT(*) FROM issue_records", conn).ExecuteScalarAsync())!;

            // Pending issues (status = 'pending' or 'requested')
            var pendingIssues = (long)(await new NpgsqlCommand(
                "SELECT COUNT(*) FROM issue_records WHERE status IN ('pending', 'requested')", conn).ExecuteScalarAsync())!;

            // Return requests awaiting approval
            var returnRequests = (long)(await new NpgsqlCommand(
                "SELECT COUNT(*) FROM issue_records WHERE return_status = 'requested'", conn).ExecuteScalarAsync())!;

            return new
            {
                totalCategories,
                totalItems,
                lowStockItems,
                totalPurchaseOrders,
                totalIssues,
                pendingIssues,
                returnRequests
            };
        }
    }
}
