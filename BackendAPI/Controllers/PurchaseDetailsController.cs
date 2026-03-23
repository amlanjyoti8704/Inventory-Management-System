using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendAPI.Models;
using System;
using System.Linq;

namespace BackendAPI.Controllers
{
    [ApiController]
    [Route("api/purchase-details")]
    public class PurchaseDetailsController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public PurchaseDetailsController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetPurchaseDetails(int itemId)
        {
            var filter = Builders<PurchaseDetailsModal>.Filter.Eq(p => p.ItemId, itemId);
            var purchaseDetails = await _context.PurchaseDetails.Find(filter).ToListAsync();

            return Ok(purchaseDetails);
        }

        // POST endpoint to insert a new purchase
        [HttpPost("{itemId}")]
        public async Task<IActionResult> AddPurchaseDetails(int itemId, [FromBody] PurchaseDetailsModal newPurchase)
        {
            try
            {
                Console.WriteLine($"🟡 Incoming request: itemId={itemId}, quantity={newPurchase.Quantity}, price={newPurchase.Price}, purchaseDate={newPurchase.PurchaseDate}");

                // Safely parse the date, or return 400
                if (!DateTime.TryParse(newPurchase.PurchaseDate, out var parsedDate))
                {
                    Console.WriteLine("❌ Invalid date format received.");
                    return BadRequest(new { message = "Invalid date format" });
                }

                var orderId = await _context.GetNextSequenceAsync("purchase_details");

                var purchase = new PurchaseDetailsModal
                {
                    OrderId = orderId,
                    ItemId = itemId,
                    Quantity = newPurchase.Quantity,
                    Price = newPurchase.Price,
                    PurchaseDate = parsedDate.ToString("yyyy-MM-dd")
                };

                await _context.PurchaseDetails.InsertOneAsync(purchase);

                return Ok(new { message = "Purchase added successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 Server Error: " + ex.Message);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeletePurchases([FromBody] DeletePurchasesRequest request)
        {
            try
            {
                if (request.OrderIds == null || request.OrderIds.Count == 0)
                    return BadRequest(new { message = "No orderIds provided." });

                try
                {
                    int totalQuantityToSubtract = 0;
                    var orderQuantities = new Dictionary<int, int>();

                    // Step 1: Validate and accumulate total quantity to subtract
                    foreach (var orderId in request.OrderIds)
                    {
                        var purchaseFilter = Builders<PurchaseDetailsModal>.Filter.And(
                            Builders<PurchaseDetailsModal>.Filter.Eq(p => p.OrderId, orderId),
                            Builders<PurchaseDetailsModal>.Filter.Eq(p => p.ItemId, request.ItemId)
                        );

                        var purchase = await _context.PurchaseDetails.Find(purchaseFilter).FirstOrDefaultAsync();

                        if (purchase == null)
                            continue;

                        totalQuantityToSubtract += purchase.Quantity;
                        orderQuantities[orderId] = purchase.Quantity;
                    }

                    // Step 2: Check available stock
                    var itemFilter = Builders<ConsumableItems>.Filter.Eq(i => i.ItemId, request.ItemId);
                    var item = await _context.ConsumableItems.Find(itemFilter).FirstOrDefaultAsync();

                    if (item == null)
                        throw new Exception("Item not found in consumableItems.");

                    if (totalQuantityToSubtract > item.Quantity)
                    {
                        return BadRequest(new
                        {
                            message = $"Cannot delete purchases. Required rollback quantity ({totalQuantityToSubtract}) exceeds current stock ({item.Quantity})."
                        });
                    }

                    // Step 3: Perform update and delete
                    foreach (var kvp in orderQuantities)
                    {
                        int orderId = kvp.Key;
                        int quantityToSubtract = kvp.Value;

                        // Update stock
                        var stockUpdate = Builders<ConsumableItems>.Update.Inc(i => i.Quantity, -quantityToSubtract);
                        await _context.ConsumableItems.UpdateOneAsync(itemFilter, stockUpdate);

                        // Delete purchase record
                        var deleteFilter = Builders<PurchaseDetailsModal>.Filter.And(
                            Builders<PurchaseDetailsModal>.Filter.Eq(p => p.OrderId, orderId),
                            Builders<PurchaseDetailsModal>.Filter.Eq(p => p.ItemId, request.ItemId)
                        );
                        await _context.PurchaseDetails.DeleteOneAsync(deleteFilter);
                    }

                    return Ok(new { message = "Selected purchases deleted and inventory updated." });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("🔥 Transaction error: " + ex.Message);
                    return StatusCode(500, new { message = "Transaction failed", error = ex.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 Server error: " + ex.Message);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}