public class ConsumableItems
{
    public int ItemId { get; set; } // Matches item_id
    public string Name { get; set; } // Matches name
    public int CategoryId { get; set; } // Matches category_id
    public int ModelNo { get; set; } // Matches model_no
    public string Brand { get; set; } // Matches brand
    public int Quantity { get; set; } // Matches quantity
    public string Storage_loc_L1 { get; set; } // Matches storage_add_l1
    public string Storage_loc_L2 { get; set; } // Matches storage_add_l2
    public DateTime? WarrantyExpiration { get; set; } // Matches warranty_expiration
}