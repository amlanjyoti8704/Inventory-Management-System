namespace BackendAPI.Models
{
    /// <summary>
    /// Maps to the "consumable_items" table in PostgreSQL.
    /// </summary>
    public class ConsumableItem
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string ModelNo { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string StorageLocL1 { get; set; } = string.Empty;
        public string StorageLocL2 { get; set; } = string.Empty;
        public DateTime? WarrantyExpiration { get; set; }
    }
}
