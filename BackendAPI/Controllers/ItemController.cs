using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly MongoDbContext _context;

    public ItemsController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAllItems()
    {
        var items = _context.ConsumableItems
            .Find(Builders<ConsumableItems>.Filter.Empty)
            .ToList();

        var result = items.Select(i => new
        {
            item_id = i.ItemId,
            name = i.Name,
            category_id = i.CategoryId,
            model_no = i.ModelNo,
            brand = i.Brand,
            quantity = i.Quantity,
            storage_loc_l1 = i.Storage_loc_L1,
            storage_loc_l2 = i.Storage_loc_L2,
            warranty_expiration = i.WarrantyExpiration.HasValue
                ? i.WarrantyExpiration.Value.ToString("yyyy-MM-dd")
                : null
        }).ToList();

        return Ok(result);
    }

    [HttpGet("purchase-details/{item_id}")]
    public IActionResult GetPurchaseDetails(int item_id)
    {
        var details = _context.PurchaseDetails
            .Find(Builders<BackendAPI.Models.PurchaseDetailsModal>.Filter.Eq(p => p.ItemId, item_id))
            .ToList();

        var result = details.Select(d => new
        {
            order_id = d.OrderId,
            quantity = d.Quantity,
            price = d.Price,
            purchase_date = d.PurchaseDate
        }).ToList();

        return Ok(result);
    }

    [HttpPost]
    public IActionResult AddItem([FromBody] dynamic item)
    {
        var newItem = new ConsumableItems
        {
            ItemId = (int)item.item_id,
            Name = (string)item.name,
            CategoryId = (int)item.category_id,
            ModelNo = (string)item.model_no,
            Brand = (string)item.brand,
            Quantity = (int)item.quantity,
            Storage_loc_L1 = (string)item.storage_loc_l1,
            Storage_loc_L2 = (string)item.storage_loc_l2,
            WarrantyExpiration = item.warranty_expiration == null ? null : (DateTime?)item.warranty_expiration
        };

        _context.ConsumableItems.InsertOne(newItem);

        return Ok(new { message = "Item added successfully" });
    }

    [HttpDelete("{item_id}")]
    public IActionResult DeleteItem(int item_id)
    {
        try
        {
            // Delete associated purchase details first
            _context.PurchaseDetails.DeleteMany(
                Builders<BackendAPI.Models.PurchaseDetailsModal>.Filter.Eq(p => p.ItemId, item_id));

            // Delete the item
            _context.ConsumableItems.DeleteOne(
                Builders<ConsumableItems>.Filter.Eq(i => i.ItemId, item_id));

            return Ok(new { message = "Item deleted with purchase details" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Delete failed", details = ex.Message });
        }
    }

    [HttpPut("update-item-with-purchase/{item_id}")]
    public IActionResult UpdateItemWithPurchase(int item_id, [FromBody] UpdateItemWithPurchaseRequest req)
    {
        try
        {
            // 1. Update item
            var itemFilter = Builders<ConsumableItems>.Filter.Eq(i => i.ItemId, item_id);
            var itemUpdate = Builders<ConsumableItems>.Update
                .Set(i => i.Name, req.Name)
                .Set(i => i.CategoryId, req.Category_Id)
                .Set(i => i.ModelNo, req.Model_No)
                .Set(i => i.Brand, req.Brand)
                .Set(i => i.Quantity, req.Quantity)
                .Set(i => i.Storage_loc_L1, req.Storage_Loc_L1)
                .Set(i => i.Storage_loc_L2, req.Storage_Loc_L2)
                .Set(i => i.WarrantyExpiration, req.Warranty_Expiration);

            _context.ConsumableItems.UpdateOne(itemFilter, itemUpdate);

            // 2. Update latest purchase_details row (if purchase data provided)
            if (req.Purchase_Price != null && req.Purchase_Quantity != null && req.Purchase_Date != null)
            {
                var purchaseFilter = Builders<BackendAPI.Models.PurchaseDetailsModal>.Filter.Eq(p => p.ItemId, item_id);
                var latestPurchase = _context.PurchaseDetails
                    .Find(purchaseFilter)
                    .SortByDescending(p => p.OrderId)
                    .Limit(1)
                    .FirstOrDefault();

                if (latestPurchase != null)
                {
                    var purchaseUpdate = Builders<BackendAPI.Models.PurchaseDetailsModal>.Update
                        .Set(p => p.Price, req.Purchase_Price.Value)
                        .Set(p => p.Quantity, req.Purchase_Quantity.Value)
                        .Set(p => p.PurchaseDate, req.Purchase_Date.Value.ToString("yyyy-MM-dd"));

                    _context.PurchaseDetails.UpdateOne(
                        Builders<BackendAPI.Models.PurchaseDetailsModal>.Filter.Eq(p => p.OrderId, latestPurchase.OrderId),
                        purchaseUpdate);
                }
            }

            return Ok(new { message = "Item and purchase updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Update failed", error = ex.Message });
        }
    }

    [HttpPost("items-with-purchase")]
    public IActionResult AddItemWithPurchase([FromBody] ItemWithPurchaseRequest request)
    {
        try
        {
            // Generate auto-increment ID for the item
            var itemId = _context.GetNextSequence("consumableItems");

            var newItem = new ConsumableItems
            {
                ItemId = itemId,
                Name = request.Item.Name,
                CategoryId = request.Item.CategoryId,
                ModelNo = request.Item.ModelNo,
                Brand = request.Item.Brand,
                Quantity = 0, // quantity handled via purchase
                Storage_loc_L1 = request.Item.StorageLocL1,
                Storage_loc_L2 = request.Item.StorageLocL2,
                WarrantyExpiration = request.Item.WarrantyExpiration == default ? null : request.Item.WarrantyExpiration
            };

            _context.ConsumableItems.InsertOne(newItem);

            // Insert purchase detail
            var orderId = _context.GetNextSequence("purchase_details");
            var purchase = new BackendAPI.Models.PurchaseDetailsModal
            {
                OrderId = orderId,
                ItemId = itemId,
                Quantity = request.Purchase.Quantity,
                Price = request.Purchase.Price,
                PurchaseDate = request.Purchase.PurchaseDate.ToString("yyyy-MM-dd")
            };

            _context.PurchaseDetails.InsertOne(purchase);

            return Ok(new { message = "Item and purchase details added successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error inserting item and purchase details", error = ex.Message });
        }
    }

    [HttpGet("items-with-purchase")]
    public IActionResult GetItemsWithPurchase()
    {
        var items = _context.ConsumableItems
            .Find(Builders<ConsumableItems>.Filter.Empty)
            .ToList();

        // Get all purchase details
        var allPurchases = _context.PurchaseDetails
            .Find(Builders<BackendAPI.Models.PurchaseDetailsModal>.Filter.Empty)
            .ToList()
            .GroupBy(p => p.ItemId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<object>();

        foreach (var item in items)
        {
            if (allPurchases.ContainsKey(item.ItemId))
            {
                foreach (var pd in allPurchases[item.ItemId])
                {
                    results.Add(new
                    {
                        item_id = item.ItemId,
                        name = item.Name,
                        category_id = item.CategoryId,
                        model_no = item.ModelNo,
                        brand = item.Brand,
                        quantity = (int?)item.Quantity,
                        storage_loc_l1 = item.Storage_loc_L1,
                        storage_loc_l2 = item.Storage_loc_L2,
                        warranty_expiration = item.WarrantyExpiration?.ToString("yyyy-MM-dd"),
                        purchase_price = (decimal?)pd.Price,
                        purchase_quantity = (int?)pd.Quantity,
                        purchase_date = pd.PurchaseDate
                    });
                }
            }
            else
            {
                // Item with no purchase details (LEFT JOIN behavior)
                results.Add(new
                {
                    item_id = item.ItemId,
                    name = item.Name,
                    category_id = item.CategoryId,
                    model_no = item.ModelNo,
                    brand = item.Brand,
                    quantity = (int?)item.Quantity,
                    storage_loc_l1 = item.Storage_loc_L1,
                    storage_loc_l2 = item.Storage_loc_L2,
                    warranty_expiration = item.WarrantyExpiration?.ToString("yyyy-MM-dd"),
                    purchase_price = (decimal?)null,
                    purchase_quantity = (int?)null,
                    purchase_date = (string?)null
                });
            }
        }

        return Ok(results);
    }
}