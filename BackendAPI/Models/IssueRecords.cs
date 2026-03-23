using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class IssueRecords
{
    [BsonId]
    [BsonElement("_id")]
    public int IssueId { get; set; } // Primary Key

    [BsonElement("issued_to")]
    public string IssuedTo { get; set; }

    [BsonElement("department")]
    public string Department { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("requested_by")]
    public string requested_by { get; set; }

    [BsonElement("status")]
    public string Status { get; set; }

    [BsonElement("return_status")]
    [BsonIgnoreIfNull]
    public string ReturnStatus { get; set; }

    [BsonElement("issue_date")]
    public DateTime IssueDate { get; set; }

    [BsonElement("item_id")]
    public int ItemId { get; set; }
}