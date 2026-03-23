using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ConsumableItems
{
    [BsonId]
    [BsonElement("_id")]
    public int ItemId { get; set; } // item_id

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("category_id")]
    public int CategoryId { get; set; }

    [BsonElement("model_no")]
    public string ModelNo { get; set; }

    [BsonElement("brand")]
    public string Brand { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("storage_loc_l1")]
    public string Storage_loc_L1 { get; set; }

    [BsonElement("storage_loc_l2")]
    public string Storage_loc_L2 { get; set; }

    [BsonElement("warranty_expiration")]
    [BsonIgnoreIfNull]
    public DateTime? WarrantyExpiration { get; set; }
}