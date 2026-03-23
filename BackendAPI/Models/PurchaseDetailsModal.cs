using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendAPI.Models
{
    public class PurchaseDetailsModal
    {
        [BsonElement("item_id")]
        public int ItemId { get; set; }

        [BsonId]
        [BsonElement("_id")]
        public int OrderId { get; set; } // Primary Key

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [BsonElement("purchase_date")]
        public string PurchaseDate { get; set; }
    }
}