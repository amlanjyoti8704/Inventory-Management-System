using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
public class AlertController : ControllerBase
{
    private readonly MongoDbContext _context;

    public AlertController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAlerts(
        [FromQuery] int? item_id,
        [FromQuery] int? category_id,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var filterBuilder = Builders<AlertLog>.Filter;
        var filters = new List<FilterDefinition<AlertLog>>();

        if (item_id.HasValue)
            filters.Add(filterBuilder.Eq(a => a.ItemId, item_id.Value));
        if (category_id.HasValue)
            filters.Add(filterBuilder.Eq(a => a.CategoryId, category_id.Value));
        if (startDate.HasValue)
            filters.Add(filterBuilder.Gte(a => a.AlertTime, startDate.Value));
        if (endDate.HasValue)
            filters.Add(filterBuilder.Lte(a => a.AlertTime, endDate.Value));

        var filter = filters.Count > 0
            ? filterBuilder.And(filters)
            : filterBuilder.Empty;

        var alertDocs = _context.AlertLogs
            .Find(filter)
            .SortByDescending(a => a.AlertTime)
            .ToList();

        // Left join with consumableItems to get item name
        var itemIds = alertDocs
            .Where(a => a.ItemId.HasValue)
            .Select(a => a.ItemId.Value)
            .Distinct()
            .ToList();

        var items = _context.ConsumableItems
            .Find(Builders<ConsumableItems>.Filter.In(i => i.ItemId, itemIds))
            .ToList()
            .ToDictionary(i => i.ItemId, i => i.Name);

        var alerts = alertDocs.Select(a => new
        {
            log_id = a.LogId.ToString(),
            item_id = a.ItemId?.ToString() ?? "N/A",
            category_id = a.CategoryId?.ToString() ?? "N/A",
            name = a.ItemId.HasValue && items.ContainsKey(a.ItemId.Value) ? items[a.ItemId.Value] : "N/A",
            category_name = a.CategoryName ?? "N/A",
            current_quantity = a.CurrentQuantity.ToString(),
            alert_message = a.AlertMessage ?? "N/A",
            alert_time = a.AlertTime.ToString("yyyy-MM-dd HH:mm:ss")
        }).ToList();

        return Ok(alerts);
    }
}
