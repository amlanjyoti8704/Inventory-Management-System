using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public DashboardController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var totalCategories = await _context.Categories.CountDocumentsAsync(Builders<Category>.Filter.Empty);
            var totalItems = await _context.ConsumableItems.CountDocumentsAsync(Builders<ConsumableItems>.Filter.Empty);

            // Low stock: items where quantity < threshold of their category
            // For simplicity, count items with quantity < 10 (a reasonable default)
            // A more accurate approach would join with categories, but MongoDB doesn't do JOINs natively
            var categories = await _context.Categories.Find(Builders<Category>.Filter.Empty).ToListAsync();
            var categoryThresholds = categories.ToDictionary(c => c.CategoryId, c => c.Threshold);
            var allItems = await _context.ConsumableItems.Find(Builders<ConsumableItems>.Filter.Empty).ToListAsync();
            var lowStockCount = allItems.Count(item =>
                categoryThresholds.ContainsKey(item.CategoryId) && item.Quantity < categoryThresholds[item.CategoryId]);

            var totalPurchaseOrders = await _context.PurchaseDetails.CountDocumentsAsync(Builders<BackendAPI.Models.PurchaseDetailsModal>.Filter.Empty);
            var totalIssues = await _context.IssueRecords.CountDocumentsAsync(Builders<IssueRecords>.Filter.Empty);

            var pendingFilter = Builders<IssueRecords>.Filter.In(i => i.Status, new[] { "pending", "requested" });
            var pendingIssues = await _context.IssueRecords.CountDocumentsAsync(pendingFilter);

            var returnFilter = Builders<IssueRecords>.Filter.Eq(i => i.ReturnStatus, "requested");
            var returnRequests = await _context.IssueRecords.CountDocumentsAsync(returnFilter);

            var summary = new
            {
                totalCategories,
                totalItems,
                lowStockItems = lowStockCount,
                totalPurchaseOrders,
                totalIssues,
                pendingIssues,
                returnRequests,
            };

            return Ok(summary);
        }
    }
}