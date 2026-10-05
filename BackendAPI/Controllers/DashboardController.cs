using Microsoft.AspNetCore.Mvc;
using BackendAPI.Services;

namespace BackendAPI.Controllers
{
    /// <summary>
    /// Dashboard summary endpoint providing KPIs for the admin dashboard.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET api/dashboard/summary — Returns aggregated dashboard statistics
        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var summary = await _dashboardService.GetSummaryAsync();
            return Ok(summary);
        }
    }
}