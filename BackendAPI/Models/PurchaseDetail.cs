namespace BackendAPI.Models
{
    /// <summary>
    /// Maps to the "purchase_details" table in PostgreSQL.
    /// </summary>
    public class PurchaseDetail
    {
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string PurchaseDate { get; set; } = string.Empty;
    }
}
