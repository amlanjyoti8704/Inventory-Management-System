using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendAPI.Models;
using System;

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
                    OrderId = reader.GetInt32(0),
                    ItemId = reader.GetInt32(1),
                    Quantity = reader.GetInt32(2),
                    Price = reader.GetDecimal(3),
                    PurchaseDate = reader.GetDateTime(4).ToString("yyyy-MM-dd")
                });
            }

            return Ok(purchaseDetails);
        }

        // ✅ New POST endpoint to insert a new purchase
        [HttpPost("{itemId}")]
        public async Task<IActionResult> AddPurchaseDetails(int itemId, [FromBody] PurchaseDetailsModal newPurchase)
        {
            try
            {
                Console.WriteLine($"🟡 Incoming request: itemId={itemId}, quantity={newPurchase.Quantity}, price={newPurchase.Price}, purchaseDate={newPurchase.PurchaseDate}");

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                string query = @"INSERT INTO purchase_details (item_id, quantity, price, purchase_date)
                         VALUES (@itemId, @quantity, @price, @purchaseDate)";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@itemId", itemId);
                command.Parameters.AddWithValue("@quantity", newPurchase.Quantity);
                command.Parameters.AddWithValue("@price", newPurchase.Price);

                // Safely parse the date, or return 400
                if (!DateTime.TryParse(newPurchase.PurchaseDate, out var parsedDate))
                {
                    Console.WriteLine("❌ Invalid date format received.");
                    return BadRequest(new { message = "Invalid date format" });
                }
                command.Parameters.AddWithValue("@purchaseDate", parsedDate);

                int result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Purchase added successfully" });
                }

                return BadRequest(new { message = "Failed to insert purchase" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 Server Error: " + ex.Message);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}