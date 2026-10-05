using BackendAPI.Data;
using BackendAPI.Models;
using Npgsql;

namespace BackendAPI.Services
{
    /// <summary>
    /// Handles all consumable item operations: listing, creation, updates, deletion.
    /// Also provides item+purchase combined views (simulating LEFT JOIN behavior).
    /// </summary>
    public class ItemService
    {
        private readonly DbHelper _db;

        public ItemService(DbHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns all items from the consumable_items table.
        /// </summary>
        public async Task<List<ConsumableItem>> GetAllAsync()
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                SELECT item_id, name, category_id, model_no, brand,
                       quantity, storage_loc_l1, storage_loc_l2, warranty_expiration
                FROM consumable_items
                ORDER BY item_id", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<ConsumableItem>();
            while (await reader.ReadAsync())
            {
                items.Add(MapItem(reader));
            }
            return items;
        }

        /// <summary>
        /// Adds a new item with a manually specified item_id.
        /// Used by the plain "Add Item" form (without purchase details).
        /// </summary>
        public async Task AddAsync(ConsumableItem item)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO consumable_items
                    (item_id, name, category_id, model_no, brand, quantity, storage_loc_l1, storage_loc_l2, warranty_expiration)
                VALUES
                    (@item_id, @name, @category_id, @model_no, @brand, @quantity, @loc1, @loc2, @warranty)", conn);

            cmd.Parameters.AddWithValue("item_id", item.ItemId);
            cmd.Parameters.AddWithValue("name", item.Name);
            cmd.Parameters.AddWithValue("category_id", item.CategoryId);
            cmd.Parameters.AddWithValue("model_no", item.ModelNo);
            cmd.Parameters.AddWithValue("brand", item.Brand);
            cmd.Parameters.AddWithValue("quantity", item.Quantity);
            cmd.Parameters.AddWithValue("loc1", item.StorageLocL1);
            cmd.Parameters.AddWithValue("loc2", item.StorageLocL2);
            cmd.Parameters.AddWithValue("warranty", (object?)item.WarrantyExpiration ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Deletes an item AND its associated purchase records (cascade delete).
        /// Purchase records are deleted first to avoid foreign key violations.
        /// </summary>
        public async Task DeleteAsync(int itemId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // Delete child records first (purchase_details reference item_id)
            using var delPurchases = new NpgsqlCommand(
                "DELETE FROM purchase_details WHERE item_id = @id", conn);
            delPurchases.Parameters.AddWithValue("id", itemId);
            await delPurchases.ExecuteNonQueryAsync();

            using var delItem = new NpgsqlCommand(
                "DELETE FROM consumable_items WHERE item_id = @id", conn);
            delItem.Parameters.AddWithValue("id", itemId);
            await delItem.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Updates item fields and optionally the latest purchase record.
        /// If purchase price/quantity/date are all provided, finds the most recent
        /// purchase for this item (by order_id DESC) and updates it.
        /// </summary>
        public async Task UpdateWithPurchaseAsync(int itemId, UpdateItemWithPurchaseRequest req)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // Step 1: Update the item itself
            using var updateItem = new NpgsqlCommand(@"
                UPDATE consumable_items
                SET name = @name, category_id = @cat, model_no = @model, brand = @brand,
                    quantity = @qty, storage_loc_l1 = @loc1, storage_loc_l2 = @loc2,
                    warranty_expiration = @warranty
                WHERE item_id = @id", conn);

            updateItem.Parameters.AddWithValue("name", req.Name);
            updateItem.Parameters.AddWithValue("cat", req.CategoryId);
            updateItem.Parameters.AddWithValue("model", req.ModelNo);
            updateItem.Parameters.AddWithValue("brand", req.Brand);
            updateItem.Parameters.AddWithValue("qty", req.Quantity);
            updateItem.Parameters.AddWithValue("loc1", req.StorageLocL1);
            updateItem.Parameters.AddWithValue("loc2", req.StorageLocL2);
            updateItem.Parameters.AddWithValue("warranty", (object?)req.WarrantyExpiration ?? DBNull.Value);
            updateItem.Parameters.AddWithValue("id", itemId);
            await updateItem.ExecuteNonQueryAsync();

            // Step 2: Only update purchase if all three purchase fields are provided
            if (req.PurchasePrice != null && req.PurchaseQuantity != null && req.PurchaseDate != null)
            {
                // Find the latest purchase for this item
                using var findLatest = new NpgsqlCommand(@"
                    SELECT order_id FROM purchase_details
                    WHERE item_id = @id ORDER BY order_id DESC LIMIT 1", conn);
                findLatest.Parameters.AddWithValue("id", itemId);

                var latestOrderId = await findLatest.ExecuteScalarAsync();
                if (latestOrderId != null)
                {
                    using var updatePurchase = new NpgsqlCommand(@"
                        UPDATE purchase_details
                        SET price = @price, quantity = @qty, purchase_date = @date
                        WHERE order_id = @orderId", conn);

                    updatePurchase.Parameters.AddWithValue("price", req.PurchasePrice.Value);
                    updatePurchase.Parameters.AddWithValue("qty", req.PurchaseQuantity.Value);
                    updatePurchase.Parameters.AddWithValue("date", (object?)req.PurchaseDate ?? DBNull.Value);
                    updatePurchase.Parameters.AddWithValue("orderId", (int)latestOrderId);
                    await updatePurchase.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Adds a new item + its first purchase record in one operation.
        /// Item quantity starts at 0 because stock is tracked via purchases.
        /// The item_id is auto-generated by PostgreSQL SERIAL.
        /// </summary>
        public async Task AddWithPurchaseAsync(ItemWithPurchaseRequest request)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // Insert item with quantity=0 (purchase adds the actual stock)
            // RETURNING item_id gives us the auto-generated ID for linking the purchase
            using var insertItem = new NpgsqlCommand(@"
                INSERT INTO consumable_items
                    (name, category_id, model_no, brand, quantity, storage_loc_l1, storage_loc_l2, warranty_expiration)
                VALUES
                    (@name, @cat, @model, @brand, @qty, @loc1, @loc2, @warranty)
                RETURNING item_id", conn);

            insertItem.Parameters.AddWithValue("name", request.Item.Name);
            insertItem.Parameters.AddWithValue("cat", request.Item.CategoryId);
            insertItem.Parameters.AddWithValue("model", request.Item.ModelNo);
            insertItem.Parameters.AddWithValue("brand", request.Item.Brand);
            insertItem.Parameters.AddWithValue("qty", request.Purchase.Quantity);
            insertItem.Parameters.AddWithValue("loc1", request.Item.StorageLocL1);
            insertItem.Parameters.AddWithValue("loc2", request.Item.StorageLocL2);
            insertItem.Parameters.AddWithValue("warranty", (object?)request.Item.WarrantyExpiration ?? DBNull.Value);

            var itemId = (int)(await insertItem.ExecuteScalarAsync())!;

            // Insert the purchase record linked to the new item
            using var insertPurchase = new NpgsqlCommand(@"
                INSERT INTO purchase_details (item_id, quantity, price, purchase_date)
                VALUES (@itemId, @qty, @price, @date)", conn);

            insertPurchase.Parameters.AddWithValue("itemId", itemId);
            insertPurchase.Parameters.AddWithValue("qty", request.Purchase.Quantity);
            insertPurchase.Parameters.AddWithValue("price", request.Purchase.Price);
            insertPurchase.Parameters.AddWithValue("date", (object?)request.Purchase.PurchaseDate ?? DBNull.Value);

            await insertPurchase.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Returns items LEFT JOINed with their purchase records.
        /// Items with no purchases still appear (with null purchase fields).
        /// This replaces the MongoDB in-memory join that was done in the old code.
        /// </summary>
        public async Task<List<object>> GetItemsWithPurchaseAsync()
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // LEFT JOIN ensures items without purchases are still returned
            using var cmd = new NpgsqlCommand(@"
                SELECT ci.item_id, ci.name, ci.category_id, ci.model_no, ci.brand,
                       ci.quantity, ci.storage_loc_l1, ci.storage_loc_l2, ci.warranty_expiration,
                       pd.price, pd.quantity AS purchase_quantity, pd.purchase_date
                FROM consumable_items ci
                LEFT JOIN purchase_details pd ON ci.item_id = pd.item_id
                ORDER BY ci.item_id", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            var results = new List<object>();
            while (await reader.ReadAsync())
            {
                results.Add(new
                {
                    item_id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    category_id = reader.GetInt32(2),
                    model_no = reader.GetString(3),
                    brand = reader.GetString(4),
                    quantity = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                    storage_loc_l1 = reader.GetString(6),
                    storage_loc_l2 = reader.GetString(7),
                    warranty_expiration = reader.IsDBNull(8) ? null : reader.GetDateTime(8).ToString("yyyy-MM-dd"),
                    purchase_price = reader.IsDBNull(9) ? (decimal?)null : reader.GetDecimal(9),
                    purchase_quantity = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10),
                    purchase_date = reader.IsDBNull(11) ? null : reader.GetString(11)
                });
            }
            return results;
        }

        /// <summary>
        /// Helper to map a data reader row to a ConsumableItem model.
        /// Reused across multiple query methods.
        /// </summary>
        private ConsumableItem MapItem(NpgsqlDataReader reader)
        {
            return new ConsumableItem
            {
                ItemId = reader.GetInt32(0),
                Name = reader.GetString(1),
                CategoryId = reader.GetInt32(2),
                ModelNo = reader.GetString(3),
                Brand = reader.GetString(4),
                Quantity = reader.GetInt32(5),
                StorageLocL1 = reader.GetString(6),
                StorageLocL2 = reader.GetString(7),
                WarrantyExpiration = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
            };
        }
    }
}
