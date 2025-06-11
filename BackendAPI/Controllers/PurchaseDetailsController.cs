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

        [HttpPost("delete")]
        public async Task<IActionResult> DeletePurchases([FromBody] DeletePurchasesRequest request)
        {
            try
            {
                if (request.OrderIds == null || request.OrderIds.Count == 0)
                    return BadRequest(new { message = "No orderIds provided." });

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                using var transaction = await connection.BeginTransactionAsync();

                try
                {
                    int totalQuantityToSubtract = 0;
                    var orderQuantities = new Dictionary<int, int>();

                    // Step 1: Validate and accumulate total quantity to subtract
                    foreach (var orderId in request.OrderIds)
                    {
                        string getQuantityQuery = "SELECT quantity FROM purchase_details WHERE order_id = @orderId AND item_id = @itemId";

                        using var getCommand = new MySqlCommand(getQuantityQuery, connection, (MySqlTransaction)transaction);
                        getCommand.Parameters.AddWithValue("@orderId", orderId);
                        getCommand.Parameters.AddWithValue("@itemId", request.ItemId);

                        var result = await getCommand.ExecuteScalarAsync();

                        if (result == null)
                            continue;

                        int quantity = Convert.ToInt32(result);
                        totalQuantityToSubtract += quantity;
                        orderQuantities[orderId] = quantity;
                    }

                    // Step 2: Check available stock
                    string checkQtyQuery = "SELECT quantity FROM consumableItems WHERE item_id = @itemId";
                    using var checkCommand = new MySqlCommand(checkQtyQuery, connection, (MySqlTransaction)transaction);
                    checkCommand.Parameters.AddWithValue("@itemId", request.ItemId);

                    var currentQtyObj = await checkCommand.ExecuteScalarAsync();
                    if (currentQtyObj == null)
                        throw new Exception("Item not found in consumableItems.");

                    int currentQty = Convert.ToInt32(currentQtyObj);
                    if (totalQuantityToSubtract > currentQty)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new
                        {
                            message = $"Cannot delete purchases. Required rollback quantity ({totalQuantityToSubtract}) exceeds current stock ({currentQty})."
                        });
                    }

                    // Step 3: Perform update and delete
                    foreach (var kvp in orderQuantities)
                    {
                        int orderId = kvp.Key;
                        int quantityToSubtract = kvp.Value;

                        string updateQuery = @"UPDATE consumableItems 
                                       SET quantity = quantity - @qty 
                                       WHERE item_id = @itemId";

                        using var updateCommand = new MySqlCommand(updateQuery, connection, (MySqlTransaction)transaction);
                        updateCommand.Parameters.AddWithValue("@qty", quantityToSubtract);
                        updateCommand.Parameters.AddWithValue("@itemId", request.ItemId);
                        await updateCommand.ExecuteNonQueryAsync();

                        string deleteQuery = "DELETE FROM purchase_details WHERE order_id = @orderId AND item_id = @itemId";

                        using var deleteCommand = new MySqlCommand(deleteQuery, connection, (MySqlTransaction)transaction);
                        deleteCommand.Parameters.AddWithValue("@orderId", orderId);
                        deleteCommand.Parameters.AddWithValue("@itemId", request.ItemId);
                        await deleteCommand.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();
                    return Ok(new { message = "Selected purchases deleted and inventory updated." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
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