using Microsoft.AspNetCore.Mvc;
using BackendAPI.Models;
using BackendAPI.Services;

namespace BackendAPI.Controllers
{
    /// <summary>
    /// Endpoints for consumable item management.
    /// Includes item CRUD, item+purchase combined operations,
    /// and purchase detail retrieval.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly ItemService _itemService;
        private readonly PurchaseService _purchaseService;

        public ItemsController(ItemService itemService, PurchaseService purchaseService)
        {
            _itemService = itemService;
            _purchaseService = purchaseService;
        }

        // GET api/items — List all items
        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _itemService.GetAllAsync();
            // Map to the response shape expected by the frontend
            var result = items.Select(i => new
            {
                item_id = i.ItemId,
                name = i.Name,
                category_id = i.CategoryId,
                model_no = i.ModelNo,
                brand = i.Brand,
                quantity = i.Quantity,
                storage_loc_l1 = i.StorageLocL1,
                storage_loc_l2 = i.StorageLocL2,
                warranty_expiration = i.WarrantyExpiration?.ToString("yyyy-MM-dd")
            });
            return Ok(result);
        }

        // GET api/items/purchase-details/{item_id} — Get purchase history for an item
        [HttpGet("purchase-details/{item_id}")]
        public async Task<IActionResult> GetPurchaseDetails(int item_id)
        {
            var details = await _purchaseService.GetByItemIdAsync(item_id);
            var result = details.Select(d => new
            {
                order_id = d.OrderId,
                quantity = d.Quantity,
                price = d.Price,
                purchase_date = d.PurchaseDate
            });
            return Ok(result);
        }

        // POST api/items — Add item (with explicit item_id)
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] ConsumableItem item)
        {
            await _itemService.AddAsync(item);
            return Ok(new { message = "Item added successfully" });
        }

        // DELETE api/items/{item_id} — Delete item + associated purchases
        [HttpDelete("{item_id}")]
        public async Task<IActionResult> DeleteItem(int item_id)
        {
            try
            {
                await _itemService.DeleteAsync(item_id);
                return Ok(new { message = "Item deleted with purchase details" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Delete failed", details = ex.Message });
            }
        }

        // PUT api/items/update-item-with-purchase/{item_id} — Update item + optionally update latest purchase
        [HttpPut("update-item-with-purchase/{item_id}")]
        public async Task<IActionResult> UpdateItemWithPurchase(int item_id, [FromBody] UpdateItemWithPurchaseRequest req)
        {
            try
            {
                await _itemService.UpdateWithPurchaseAsync(item_id, req);
                return Ok(new { message = "Item and purchase updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Update failed", error = ex.Message });
            }
        }

        // POST api/items/items-with-purchase — Add new item + first purchase in one call
        [HttpPost("items-with-purchase")]
        public async Task<IActionResult> AddItemWithPurchase([FromBody] ItemWithPurchaseRequest request)
        {
            try
            {
                await _itemService.AddWithPurchaseAsync(request);
                return Ok(new { message = "Item and purchase details added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error inserting item and purchase details", error = ex.Message });
            }
        }

        // GET api/items/items-with-purchase — List items LEFT JOINed with purchases
        [HttpGet("items-with-purchase")]
        public async Task<IActionResult> GetItemsWithPurchase()
        {
            var results = await _itemService.GetItemsWithPurchaseAsync();
            return Ok(results);
        }
    }
}