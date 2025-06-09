using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using MySqlConnector;
using System.Data;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly DbContext _context;
    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration)
    {
        _configuration = configuration;
        _context = new DbContext(_configuration);
    }

    [HttpPost("signup")]
    public IActionResult Signup(User user)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        using var conn = _context.GetConnection();
        conn.Open();

        var cmd = new MySqlCommand(
            "INSERT INTO Users (email, username, password, role, createdAt) VALUES (@email, @username, @password, @role, @createdAt)",
            conn);

        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@password", user.Password);
        cmd.Parameters.AddWithValue("@role", string.IsNullOrEmpty(user.Role) ? "user" : user.Role);
        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);

        cmd.ExecuteNonQuery();

        // Fetch the inserted user (including ID)
        var getUserCmd = new MySqlCommand("SELECT id, username, email, role FROM Users WHERE email = @Email", conn);
        getUserCmd.Parameters.AddWithValue("@Email", user.Email);

        using var reader = getUserCmd.ExecuteReader();
        if (reader.Read())
        {
            return Ok(new
            {
                message = "User created",
                user = new
                {
                    id = reader["id"],
                    username = reader["username"],
                    email = reader["email"],
                    role = reader["role"]
                }
            });
        }

        return StatusCode(500, new { message = "User created but failed to retrieve user data" });
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest login)
    {
        using var conn = _context.GetConnection();
        conn.Open();

        var cmd = new MySqlCommand("SELECT * FROM Users WHERE email = @Email", conn);
        cmd.Parameters.AddWithValue("@Email", login.Email);

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            string storedHash = reader["password"].ToString();
            if (BCrypt.Net.BCrypt.Verify(login.Password, storedHash))
            {
                return Ok(new
                {
                    message = "Login successful",
                    user = new
                    {
                        id = reader["id"],
                        username = reader["username"],
                        email = reader["email"],
                        role = reader["role"]
                    }
                });
            }
        }

        return Unauthorized(new { message = "Invalid credentials" });
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = new List<User>();

        using var conn = _context.GetConnection();
        conn.Open();

        var cmd = new MySqlCommand("SELECT id, username, email, role FROM Users", conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            users.Add(new User
            {
                Id = Convert.ToInt32(reader["id"]),
                Username = reader["username"].ToString(),
                Email = reader["email"].ToString(),
                Role = reader["role"].ToString()
            });
        }

        return Ok(users);
    }

    [HttpPut("update-role")]
    public async Task<IActionResult> UpdateUserRole([FromBody] RoleUpdateRequest request)
    {
        using var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        var query = "UPDATE Users SET role = @Role WHERE email = @Email";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Role", request.Role);
        command.Parameters.AddWithValue("@Email", request.Email);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected > 0)
            return Ok(new { message = "Role updated successfully" });
        else
            return NotFound(new { message = "User not found" });
    }

    [HttpGet("me")]
    public IActionResult GetUserByEmail([FromQuery] string email)
    {
        using var conn = _context.GetConnection();
        conn.Open();

        var cmd = new MySqlCommand("SELECT id, username, email, role FROM Users WHERE email = @Email", conn);
        cmd.Parameters.AddWithValue("@Email", email);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return Ok(new
            {
                id = reader["id"],
                username = reader["username"],
                email = reader["email"],
                role = reader["role"]
            });
        }

        return NotFound(new { message = "User not found" });
    }

    // [HttpPost("forgot-password")]
    // public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
    // {
    //     if (string.IsNullOrWhiteSpace(request.Email))
    //         return BadRequest(new { message = "Email is required" });

    //     var db = new DbContext();
    //     using var conn = db.GetConnection();
    //     conn.Open();

    //     // 1. Check if user exists
    //     var checkCmd = new MySqlCommand("SELECT * FROM Users WHERE Email = @Email", conn);
    //     checkCmd.Parameters.AddWithValue("@Email", request.Email);

    //     using var reader = checkCmd.ExecuteReader();
    //     if (!reader.HasRows)
    //         return NotFound(new { message = "User not found" });

    //     reader.Close();

    //     // 2. Generate secure token
    //     var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    //     var expiry = DateTime.UtcNow.AddHours(1);

    //     // 3. Update user with token + expiry
    //     var updateCmd = new MySqlCommand(
    //         "UPDATE Users SET ResetToken = @Token, ResetTokenExpiry = @Expiry WHERE Email = @Email", conn);
    //     updateCmd.Parameters.AddWithValue("@Token", token);
    //     updateCmd.Parameters.AddWithValue("@Expiry", expiry);
    //     updateCmd.Parameters.AddWithValue("@Email", request.Email);
    //     updateCmd.ExecuteNonQuery();

    //     // 4. Simulate sending email
    //     var resetLink = $"http://localhost:5173/reset-password?token={token}";
    //     // Console.WriteLine($"Reset password link: {resetLink}");

    //     var email = new MimeMessage();
    //     email.From.Add(MailboxAddress.Parse("your@email.com"));
    //     email.To.Add(MailboxAddress.Parse(request.Email));
    //     email.Subject = "Password Reset Link";

    //     email.Body = new TextPart("plain")
    //     {
    //         Text = $"Click the following link to reset your password: {resetLink}"
    //     };

    //     using var smtp = new SmtpClient();
    //     smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
    //     smtp.Authenticate("your@email.com", "your_email_app_password"); // NOT your Gmail password directly
    //     smtp.Send(email);
    //     smtp.Disconnect(true);

    //     return Ok(new { message = "Reset password link has been generated." });
    // }

    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        var db = new DbContext(_configuration);
        using var conn = db.GetConnection();
        conn.Open();

        // 1. Check if user exists
        var checkCmd = new MySqlCommand("SELECT * FROM Users WHERE Email = @Email", conn);
        checkCmd.Parameters.AddWithValue("@Email", request.Email);

        using var reader = checkCmd.ExecuteReader();
        if (!reader.HasRows)
            return NotFound(new { message = "User not found" });

        reader.Close();

        // 2. Generate secure token
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiry = DateTime.UtcNow.AddHours(1);

        // 3. Update user with token + expiry
        var updateCmd = new MySqlCommand(
            "UPDATE Users SET ResetToken = @Token, ResetTokenExpiry = @Expiry WHERE Email = @Email", conn);
        updateCmd.Parameters.AddWithValue("@Token", token);
        updateCmd.Parameters.AddWithValue("@Expiry", expiry);
        updateCmd.Parameters.AddWithValue("@Email", request.Email);
        updateCmd.ExecuteNonQuery();

        // 4. Send email using Mailtrap
        var resetLink = $"http://localhost:5173/reset-password?token={token}";

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("no-reply@example.com")); // This can be any email address
        email.To.Add(MailboxAddress.Parse(request.Email));
        email.Subject = "Password Reset Request";
        email.Body = new TextPart("plain")
        {
            Text = $"You requested a password reset.\nClick this link to reset your password:\n{resetLink}"
        };

        using var smtp = new SmtpClient();
        smtp.Connect("sandbox.smtp.mailtrap.io", 587, MailKit.Security.SecureSocketOptions.StartTls);

        // ✅ Replace with your actual Mailtrap SMTP credentials
        smtp.Authenticate("d759da824c044c", "9233fb3dc20b92");

        smtp.Send(email);
        smtp.Disconnect(true);

        return Ok(new
        {
            message = "Reset password link has been sent to your email.",
            token = token //remove this line in production
         });
    }


    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var db = new DbContext(_configuration);
        using var conn = db.GetConnection();
        conn.Open();

        var checkCmd = new MySqlCommand("SELECT * FROM Users WHERE ResetToken = @Token AND ResetTokenExpiry > @Now", conn);
        checkCmd.Parameters.AddWithValue("@Token", req.Token);
        checkCmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);

        using var reader = checkCmd.ExecuteReader();
        if (!reader.HasRows)
            return BadRequest(new { message = "Invalid or expired token." });

        reader.Close();

        // Hash password
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

        var updateCmd = new MySqlCommand("UPDATE Users SET Password = @Password, ResetToken = NULL, ResetTokenExpiry = NULL WHERE ResetToken = @Token", conn);
        updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
        updateCmd.Parameters.AddWithValue("@Token", req.Token);
        updateCmd.ExecuteNonQuery();

        return Ok(new { message = "Password reset successful." });
    }


    public class ResetPasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

    [HttpPost("request-password-reset")]
    public IActionResult RequestPasswordReset([FromBody] PhoneRequest request)
    {
        var db = new DbContext(_configuration);
        using var conn = db.GetConnection();
        conn.Open();

        var checkCmd = new MySqlCommand("SELECT * FROM Users WHERE PhoneNumber = @Phone", conn);
        checkCmd.Parameters.AddWithValue("@Phone", request.PhoneNumber);

        using var reader = checkCmd.ExecuteReader();
        if (!reader.Read())
            return NotFound(new { message = "User not found" });

        reader.Close();

        // Generate OTP token
        var token = new Random().Next(100000, 999999).ToString(); // 6-digit code
        var expiry = DateTime.UtcNow.AddMinutes(5);

        var updateCmd = new MySqlCommand("UPDATE Users SET ResetToken = @Token, ResetTokenExpiry = @Expiry WHERE PhoneNumber = @Phone", conn);
        updateCmd.Parameters.AddWithValue("@Token", token);
        updateCmd.Parameters.AddWithValue("@Expiry", expiry);
        updateCmd.Parameters.AddWithValue("@Phone", request.PhoneNumber);
        updateCmd.ExecuteNonQuery();

        // Send SMS using Twilio
        var accountSid = "AC58b121fb266609475e502722eaad7390";
        var authToken = "45c04b1a1ceb4fcb718bb27eb5308b5d";
        // Replace with your Twilio phone number
        //But Twilio phone number needed to be bought for $1.15/month so for later purposes
        var twilioPhone = "+1234567890";

        TwilioClient.Init(accountSid, authToken);

        var phone = request.PhoneNumber;
        if (!phone.StartsWith("+"))
        {
            phone = "+91" + phone; // Assuming you're only handling Indian numbers
        }

        var message = MessageResource.Create(
            body: $"Your password reset code is {token}",
            from: new Twilio.Types.PhoneNumber(twilioPhone),
            to: new Twilio.Types.PhoneNumber(phone)
        );

        return Ok(new { message = "OTP sent to phone" });
    }

    public class PhoneRequest
    {
        public string PhoneNumber { get; set; }
    }

    [HttpPost("verify-token-reset")]
    public IActionResult VerifyTokenReset([FromBody] VerifyTokenRequest req)
    {
        var db = new DbContext(_configuration);
        using var conn = db.GetConnection();
        conn.Open();

        var checkCmd = new MySqlCommand("SELECT * FROM Users WHERE PhoneNumber = @Phone AND ResetToken = @Token AND ResetTokenExpiry > @Now", conn);
        checkCmd.Parameters.AddWithValue("@Phone", req.PhoneNumber);
        checkCmd.Parameters.AddWithValue("@Token", req.Token);
        checkCmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);

        using var reader = checkCmd.ExecuteReader();
        if (!reader.Read())
            return BadRequest(new { message = "Invalid or expired token." });

        reader.Close();

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

        var updateCmd = new MySqlCommand("UPDATE Users SET Password = @Password, ResetToken = NULL, ResetTokenExpiry = NULL WHERE PhoneNumber = @Phone", conn);
        updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
        updateCmd.Parameters.AddWithValue("@Phone", req.PhoneNumber);
        updateCmd.ExecuteNonQuery();

        return Ok(new { message = "Password reset successful." });
    }
    public class VerifyTokenRequest
    {
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

}