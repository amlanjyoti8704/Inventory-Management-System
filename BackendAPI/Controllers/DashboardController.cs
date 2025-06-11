using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            using var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var summary = new
            {
                totalCategories = await ExecuteCountQuery(connection, "SELECT COUNT(*) FROM category"),
                totalItems = await ExecuteCountQuery(connection, "SELECT COUNT(*) FROM consumableItems"),
                lowStockItems = await ExecuteCountQuery(connection, @"select count(*) from xyz;"),
                // totalSuppliers = await ExecuteCountQuery(connection, "SELECT COUNT(*) FROM suppliers"),
                totalPurchaseOrders = await ExecuteCountQuery(connection, "SELECT COUNT(*) FROM purchase_details"),
                totalIssues = await ExecuteCountQuery(connection, "SELECT COUNT(*) FROM issue_records"),
                pendingIssues = await ExecuteCountQuery(connection, "SELECT COUNT(*) FROM issue_records where status='pending' or status='requested'"),
                returnRequests = await ExecuteCountQuery(connection, "SELECT COUNT(*) FROM issue_records WHERE return_status = 'requested'"),
            };

            return Ok(summary);
        }

        private async Task<int> ExecuteCountQuery(MySqlConnection connection, string query)
        {
            using var command = new MySqlCommand(query, connection);
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}