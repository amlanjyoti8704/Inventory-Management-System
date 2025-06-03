public class UpdateItemWithPurchaseRequest
{
    public string Name { get; set; }
    public int Category_Id { get; set; }
    public string Model_No { get; set; }  // If it's a string in your frontend
    public string Brand { get; set; }
    public int Quantity { get; set; }
    public string Storage_Loc_L1 { get; set; }
    public string Storage_Loc_L2 { get; set; }
    public DateTime? Warranty_Expiration { get; set; }
    public decimal? Purchase_Price { get; set; }
    public int? Purchase_Quantity { get; set; }
    public DateTime? Purchase_Date { get; set; }
}