public class ItemWithPurchaseRequest
{
    public ItemDto Item { get; set; }
    public PurchaseDto Purchase { get; set; }
}

public class ItemDto
{
    public string Name { get; set; }
    public int CategoryId { get; set; }
    public string ModelNo { get; set; }
    public string Brand { get; set; }
    public int Quantity { get; set; }
    public string StorageLocL1 { get; set; }
    public string StorageLocL2 { get; set; }
}

public class PurchaseDto
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; }
}