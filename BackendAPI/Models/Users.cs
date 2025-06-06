public class User
{
    public int Id { get; set; } // Primary Key
    public string Username { get; set; } // Unique
    public string Email { get; set; } // Unique
    public string Password { get; set; } // Hashed
    public string Role { get; set; } = "user"; // NEW: user/admin
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Timestamp

    //added for password reset functionality
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
    public string? PhoneNumber { get; set; }
}