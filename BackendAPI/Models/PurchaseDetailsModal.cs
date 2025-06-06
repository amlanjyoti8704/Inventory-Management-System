using System;

namespace BackendAPI.Models
{
    public class PurchaseDetailsModal
    {
        public int ItemId { get; set; } // Matches item_id
        public int OrderId { get; set; } // Matches order_id (Primary Key)
        public int Quantity { get; set; } // Matches quantity
        public decimal Price { get; set; } // Matches price (decimal(9,2))
        public string PurchaseDate { get; set; } // Change type to string if formatted in controller
    }
}