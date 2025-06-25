using Microsoft.AspNetCore.Mvc;
// using MySql.Data.MySqlClient;
using MySqlConnector;
using System.Data;
using System.Collections.Generic;

[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly string _connectionString;

    public ItemsController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }    // private string connectionString = "server=localhost;user=amlan;password=test1234;database=IT_consumables;";

    [HttpGet]
    public IActionResult GetAllItems()
    {
        var items = new List<dynamic>();

        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            var query = "SELECT * FROM consumableItems";
            var cmd = new MySqlCommand(query, connection);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new
                {
                    item_id = reader["item_id"],
                    name = reader["name"],
                    category_id = reader["category_id"],
                    model_no = reader["model_no"],
                    brand = reader["brand"],
                    quantity = reader["quantity"],
                    storage_loc_l1 = reader["storage_loc_l1"],
                    storage_loc_l2 = reader["storage_loc_l2"],
                    warranty_expiration = reader["warranty_expiration"] == DBNull.Value ? null : ((DateTime)reader["warranty_expiration"]).ToString("yyyy-MM-dd")
                });
            }
        }

        return Ok(items);
    }

    [HttpGet("purchase-details/{item_id}")]
    public IActionResult GetPurchaseDetails(int item_id)
    {
        var details = new List<dynamic>();

        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            var query = "SELECT * FROM purchase_details WHERE item_id = @item_id";
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@item_id", item_id);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                details.Add(new
                {
                    order_id = reader["order_id"],
                    quantity = reader["quantity"],
                    price = reader["price"],
                    purchase_date = ((DateTime)reader["purchase_date"]).ToString("yyyy-MM-dd")
                });
            }
        }

        return Ok(details);
    }

    [HttpPost]
    public IActionResult AddItem([FromBody] dynamic item)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            var query = @"INSERT INTO consumableItems 
                (item_id, name, category_id, model_no, brand, quantity, storage_loc_l1, storage_loc_l2, warranty_expiration) 
                VALUES (@item_id, @name, @category_id, @model_no, @brand, @quantity, @storage_loc_l1, @storage_loc_l2, @warranty_expiration)";

            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@item_id", (int)item.item_id);
            cmd.Parameters.AddWithValue("@name", (string)item.name);
            cmd.Parameters.AddWithValue("@category_id", (int)item.category_id);
            cmd.Parameters.AddWithValue("@model_no", (int)item.model_no);
            cmd.Parameters.AddWithValue("@brand", (string)item.brand);
            cmd.Parameters.AddWithValue("@quantity", (int)item.quantity);
            cmd.Parameters.AddWithValue("@storage_loc_l1", (string)item.storage_loc_l1);
            cmd.Parameters.AddWithValue("@storage_loc_l2", (string)item.storage_loc_l2);
            cmd.Parameters.AddWithValue("@warranty_expiration", item.warranty_expiration == null ? DBNull.Value : (object)item.warranty_expiration);

            cmd.ExecuteNonQuery();
        }

        return Ok(new { message = "Item added successfully" });
    }


    [HttpDelete("{item_id}")]
    public IActionResult DeleteItem(int item_id)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var cmd1 = new MySqlCommand("DELETE FROM purchase_details WHERE item_id = @item_id", connection, transaction);
            cmd1.Parameters.AddWithValue("@item_id", item_id);
            cmd1.ExecuteNonQuery();

            var cmd2 = new MySqlCommand("DELETE FROM consumableItems WHERE item_id = @item_id", connection, transaction);
            cmd2.Parameters.AddWithValue("@item_id", item_id);
            int rows = cmd2.ExecuteNonQuery();

            transaction.Commit();
            return Ok(new { message = "Item deleted with purchase details" });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, new { error = "Delete failed", details = ex.Message });
        }
    }


    [HttpPut("update-item-with-purchase/{item_id}")]
    public IActionResult UpdateItemWithPurchase(int item_id, [FromBody] UpdateItemWithPurchaseRequest req)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            // Begin transaction
            using var transaction = connection.BeginTransaction();

            // 1. Update item
            var updateItemQuery = @"UPDATE consumableItems SET 
            name = @name,
            category_id = @category_id,
            model_no = @model_no,
            brand = @brand,
            quantity = @quantity,
            storage_loc_l1 = @storage_loc_l1,
            storage_loc_l2 = @storage_loc_l2,
            warranty_expiration = @warranty_expiration
        WHERE item_id = @item_id";

            using var cmd1 = new MySqlCommand(updateItemQuery, connection, transaction);
            cmd1.Parameters.AddWithValue("@name", req.Name);
            cmd1.Parameters.AddWithValue("@category_id", req.Category_Id);
            cmd1.Parameters.AddWithValue("@model_no", req.Model_No);
            cmd1.Parameters.AddWithValue("@brand", req.Brand);
            cmd1.Parameters.AddWithValue("@quantity", req.Quantity);
            cmd1.Parameters.AddWithValue("@storage_loc_l1", req.Storage_Loc_L1);
            cmd1.Parameters.AddWithValue("@storage_loc_l2", req.Storage_Loc_L2);
            cmd1.Parameters.AddWithValue("@warranty_expiration", req.Warranty_Expiration ?? (object)DBNull.Value);
            cmd1.Parameters.AddWithValue("@item_id", item_id);
            cmd1.ExecuteNonQuery();

            // 2. Update latest purchase_details row (if exists)
            if (req.Purchase_Price != null && req.Purchase_Quantity != null && req.Purchase_Date != null)
            {
                var updatePurchaseQuery = @"UPDATE purchase_details 
                SET price = @price, quantity = @purchase_quantity, purchase_date = @purchase_date 
                WHERE item_id = @item_id 
                ORDER BY purchase_id DESC LIMIT 1";

                using var cmd2 = new MySqlCommand(updatePurchaseQuery, connection, transaction);
                cmd2.Parameters.AddWithValue("@price", req.Purchase_Price);
                cmd2.Parameters.AddWithValue("@purchase_quantity", req.Purchase_Quantity);
                cmd2.Parameters.AddWithValue("@purchase_date", req.Purchase_Date);
                cmd2.Parameters.AddWithValue("@item_id", item_id);
                cmd2.ExecuteNonQuery();
            }

            transaction.Commit();
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
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            // Insert item
            var insertItemCmd = new MySqlCommand(
                @"INSERT INTO consumableItems 
                    (name, category_id, model_no, brand, quantity, storage_loc_l1, storage_loc_l2, warranty_expiration) 
                VALUES 
                    (@name, @category_id, @model_no, @brand, @quantity, @storage_loc_l1, @storage_loc_l2, @warranty_expiration);
                SELECT LAST_INSERT_ID();", connection, transaction);

            insertItemCmd.Parameters.AddWithValue("@name", request.Item.Name);
            insertItemCmd.Parameters.AddWithValue("@category_id", request.Item.CategoryId);
            insertItemCmd.Parameters.AddWithValue("@model_no", request.Item.ModelNo);
            insertItemCmd.Parameters.AddWithValue("@brand", request.Item.Brand);
            insertItemCmd.Parameters.AddWithValue("@quantity", request.Item.Quantity);
            insertItemCmd.Parameters.AddWithValue("@storage_loc_l1", request.Item.StorageLocL1);
            insertItemCmd.Parameters.AddWithValue("@storage_loc_l2", request.Item.StorageLocL2);
            insertItemCmd.Parameters.AddWithValue("@warranty_expiration", request.Item.WarrantyExpiration == null ? DBNull.Value : (object)request.Item.WarrantyExpiration);

            var itemId = Convert.ToInt32(insertItemCmd.ExecuteScalar());

            // Insert purchase detail
            var insertPurchaseCmd = new MySqlCommand(
                @"INSERT INTO purchase_details 
                    (item_id, quantity, price, purchase_date) 
                VALUES 
                    (@item_id, @quantity, @price, @purchase_date);", connection, transaction);

            insertPurchaseCmd.Parameters.AddWithValue("@item_id", itemId);
            insertPurchaseCmd.Parameters.AddWithValue("@quantity", request.Purchase.Quantity);
            insertPurchaseCmd.Parameters.AddWithValue("@price", request.Purchase.Price);
            insertPurchaseCmd.Parameters.AddWithValue("@purchase_date", request.Purchase.PurchaseDate);

            insertPurchaseCmd.ExecuteNonQuery();

            transaction.Commit();
            return Ok(new { message = "Item and purchase details added successfully." });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, new { message = "Error inserting item and purchase details", error = ex.Message });
        }
    }

    
    [HttpGet("items-with-purchase")]
    public IActionResult GetItemsWithPurchase()
    {
        var results = new List<dynamic>();

        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            var query = @"
                SELECT 
                    ci.item_id, ci.name, ci.category_id, ci.model_no, ci.brand, ci.quantity AS item_quantity, 
                    ci.storage_loc_l1, ci.storage_loc_l2, ci.warranty_expiration,
                    pd.purchase_date, pd.quantity AS purchase_quantity, pd.price
                FROM consumableItems ci
                LEFT JOIN purchase_details pd ON ci.item_id = pd.item_id";

            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                results.Add(new
                {
                    item_id = reader["item_id"],
                    name = reader["name"],
                    category_id = reader["category_id"],
                    model_no = reader["model_no"],
                    brand = reader["brand"],
                    quantity = reader["item_quantity"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["item_quantity"]),
                    storage_loc_l1 = reader["storage_loc_l1"],
                    storage_loc_l2 = reader["storage_loc_l2"],
                    warranty_expiration = reader["warranty_expiration"] == DBNull.Value ? null : ((DateTime)reader["warranty_expiration"]).ToString("yyyy-MM-dd"),
                    purchase_price = reader["price"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["price"]),
                    purchase_quantity = reader["purchase_quantity"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["purchase_quantity"]),
                    purchase_date = reader["purchase_date"] == DBNull.Value ? null : ((DateTime)reader["purchase_date"]).ToString("yyyy-MM-dd")
                });
            }
        }

        return Ok(results);
    }
}