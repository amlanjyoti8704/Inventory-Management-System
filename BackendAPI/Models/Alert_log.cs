using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class AlertLog
{
    [BsonId]
    [BsonElement("_id")]
    public int LogId { get; set; } // Primary Key

    [BsonElement("item_id")]
    [BsonIgnoreIfNull]
    public int? ItemId { get; set; }

    [BsonElement("category_id")]
    [BsonIgnoreIfNull]
    public int? CategoryId { get; set; }

    [BsonElement("category_name")]
    [BsonIgnoreIfNull]
    public string? CategoryName { get; set; }

    [BsonElement("current_quantity")]
    public int CurrentQuantity { get; set; }

    [BsonElement("alert_message")]
    public string AlertMessage { get; set; }

    [BsonElement("alert_time")]
    public DateTime AlertTime { get; set; }
}