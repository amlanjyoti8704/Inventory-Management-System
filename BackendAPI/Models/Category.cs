using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Category
{
    [BsonId]
    [BsonElement("_id")]
    public int CategoryId { get; set; } // Primary Key

    [BsonElement("category_name")]
    public string CategoryName { get; set; }

    [BsonElement("threshold")]
    public int Threshold { get; set; }
}