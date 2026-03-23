using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    [BsonElement("_id")]
    public int Id { get; set; } // Primary Key

    [BsonElement("username")]
    public string Username { get; set; } // Unique

    [BsonElement("email")]
    public string Email { get; set; } // Unique

    [BsonElement("password")]
    public string Password { get; set; } // Hashed

    [BsonElement("role")]
    public string Role { get; set; } = "user"; // user/admin

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    //added for password reset functionality
    [BsonElement("resetToken")]
    [BsonIgnoreIfNull]
    public string? ResetToken { get; set; }

    [BsonElement("resetTokenExpiry")]
    [BsonIgnoreIfNull]
    public DateTime? ResetTokenExpiry { get; set; }

    [BsonElement("phoneNumber")]
    [BsonIgnoreIfNull]
    public string? PhoneNumber { get; set; }
}