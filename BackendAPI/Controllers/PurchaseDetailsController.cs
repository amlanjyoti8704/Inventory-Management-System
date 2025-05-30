using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendAPI.Models;

namespace BackendAPI.Controllers
{
    [ApiController]
    [Route("api/purchase-details")]
    public class PurchaseDetailsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PurchaseDetailsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetPurchaseDetails(int itemId)
        {
            var purchaseDetails = new List<PurchaseDetailsModal>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT order_id, item_id, quantity, price, purchase_date FROM purchase_details WHERE item_id = @itemId";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@itemId", itemId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                purchaseDetails.Add(new PurchaseDetailsModal
                {
                    OrderId = reader.GetInt32(0),            // order_id
                    ItemId = reader.GetInt32(1),             // item_id
                    Quantity = reader.GetInt32(2),           // quantity
                    Price = reader.GetDecimal(3),            // price
                    PurchaseDate = reader.GetDateTime(4).ToString("yyyy-MM-dd") // purchase_date
                });
            }

            return Ok(purchaseDetails);
        }
    }
}