// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc;
// using MySqlConnector;
// using BackendAPI.Data; // replace with the actual namespace of your DbContextusing System;
// using System.Security.Cryptography;
// using System.Text;
// using System.Threading.Tasks;

// [ApiController]
// [Route("api/user")]
// public class UserController : ControllerBase
// {
//     private readonly AppDbContext _context;

//     public UserController(AppDbContext context)
//     {
//         _context = context;
//     }

//     // POST: /api/user/forgot-password
//    [HttpPost("forgot-password")]
//     public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
//     {
//         if (string.IsNullOrWhiteSpace(request.Email))
//             return BadRequest(new { message = "Email is required" });

//         var db = new DbContext();
//         using var conn = db.GetConnection();
//         conn.Open();

//         // 1. Check if user exists
//         var checkCmd = new MySqlCommand("SELECT * FROM Users WHERE Email = @Email", conn);
//         checkCmd.Parameters.AddWithValue("@Email", request.Email);

//         using var reader = checkCmd.ExecuteReader();
//         if (!reader.HasRows)
//             return NotFound(new { message = "User not found" });

//         reader.Close();

//         // 2. Generate secure token
//         var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
//         var expiry = DateTime.UtcNow.AddHours(1);

//         // 3. Update user with token + expiry
//         var updateCmd = new MySqlCommand(
//             "UPDATE Users SET ResetToken = @Token, ResetTokenExpiry = @Expiry WHERE Email = @Email", conn);
//         updateCmd.Parameters.AddWithValue("@Token", token);
//         updateCmd.Parameters.AddWithValue("@Expiry", expiry);
//         updateCmd.Parameters.AddWithValue("@Email", request.Email);
//         updateCmd.ExecuteNonQuery();

//         // 4. Simulate sending email
//         var resetLink = $"http://localhost:5173/reset-password?token={token}";
//         Console.WriteLine($"Reset password link: {resetLink}");

//         return Ok(new { message = "Reset password link has been generated." });
//     }

// }