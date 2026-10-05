using Microsoft.AspNetCore.Mvc;
using BackendAPI.Models;
using BackendAPI.Services;

namespace BackendAPI.Controllers
{
    /// <summary>
    /// Manages purchase detail records for items.
    /// Supports adding purchases and batch-deleting with stock rollback.
    /// </summary>
    [ApiController]
    [Route("api/purchase-details")]
    public class PurchaseDetailsController : ControllerBase
    {
        private readonly PurchaseService _purchaseService;

        public PurchaseDetailsController(PurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        // GET api/purchase-details/{itemId} — Get all purchases for an item
        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetPurchaseDetails(int itemId)
        {
            var details = await _purchaseService.GetByItemIdAsync(itemId);
            return Ok(details);
        }

        // POST api/purchase-details/{itemId} — Add a purchase for an item
        [HttpPost("{itemId}")]
        public async Task<IActionResult> AddPurchaseDetails(int itemId, [FromBody] PurchaseDetail newPurchase)
        {
            try
            {
                // Validate date format before persisting
                if (!DateTime.TryParse(newPurchase.PurchaseDate, out var parsedDate))
                    return BadRequest(new { message = "Invalid date format" });

                newPurchase.PurchaseDate = parsedDate.ToString("yyyy-MM-dd");
                await _purchaseService.AddAsync(itemId, newPurchase);

                return Ok(new { message = "Purchase added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // POST api/purchase-details/delete — Batch delete purchases with stock rollback
        [HttpPost("delete")]
        public async Task<IActionResult> DeletePurchases([FromBody] DeletePurchasesRequest request)
        {
            try
            {
                if (request.OrderIds == null || request.OrderIds.Count == 0)
                    return BadRequest(new { message = "No orderIds provided." });

                var (success, error) = await _purchaseService.DeletePurchasesAsync(request);
                if (!success)
                    return BadRequest(new { message = error });

                return Ok(new { message = "Selected purchases deleted and inventory updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}