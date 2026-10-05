using Microsoft.AspNetCore.Mvc;
using BackendAPI.Services;

namespace BackendAPI.Controllers
{
    /// <summary>
    /// Alert log endpoints. Supports filtering by item, category, and date range.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private readonly AlertService _alertService;

        public AlertController(AlertService alertService)
        {
            _alertService = alertService;
        }

        // GET api/alert?item_id=&category_id=&startDate=&endDate=
        [HttpGet]
        public async Task<IActionResult> GetAlerts(
            [FromQuery] int? item_id,
            [FromQuery] int? category_id,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var alerts = await _alertService.GetAlertsAsync(item_id, category_id, startDate, endDate);
            return Ok(alerts);
        }
    }
}
