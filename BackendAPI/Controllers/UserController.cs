using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using MongoDB.Driver;
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
    private readonly MongoDbContext _context;
    private readonly IConfiguration _configuration;

    public UserController(MongoDbContext context, IConfiguration configuration)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("signup")]
    public IActionResult Signup(User user)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        user.Id = _context.GetNextSequence("Users");
        user.Role = string.IsNullOrEmpty(user.Role) ? "user" : user.Role;
        user.CreatedAt = DateTime.Now;

        _context.Users.InsertOne(user);

        return Ok(new
        {
            message = "User created",
            user = new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role
            }
        });
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest login)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, login.Email);
        var user = _context.Users.Find(filter).FirstOrDefault();

        if (user != null)
        {
            if (BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
            {
                return Ok(new
                {
                    message = "Login successful",
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        email = user.Email,
                        role = user.Role
                    }
                });
            }
        }

        return Unauthorized(new { message = "Invalid credentials" });
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = _context.Users
            .Find(Builders<User>.Filter.Empty)
            .ToList();

        var result = users.Select(u => new User
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role
        }).ToList();

        return Ok(result);
    }

    [HttpPut("update-role")]
    public async Task<IActionResult> UpdateUserRole([FromBody] RoleUpdateRequest request)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, request.Email);
        var update = Builders<User>.Update.Set(u => u.Role, request.Role);

        var result = await _context.Users.UpdateOneAsync(filter, update);

        if (result.ModifiedCount > 0)
            return Ok(new { message = "Role updated successfully" });
        else
            return NotFound(new { message = "User not found" });
    }

    [HttpGet("me")]
    public IActionResult GetUserByEmail([FromQuery] string email)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        var user = _context.Users.Find(filter).FirstOrDefault();

        if (user != null)
        {
            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role
            });
        }

        return NotFound(new { message = "User not found" });
    }

    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });

        // 1. Check if user exists
        var filter = Builders<User>.Filter.Eq(u => u.Email, request.Email);
        var user = _context.Users.Find(filter).FirstOrDefault();

        if (user == null)
            return NotFound(new { message = "User not found" });

        // 2. Generate secure token
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiry = DateTime.UtcNow.AddHours(1);

        // 3. Update user with token + expiry
        var update = Builders<User>.Update
            .Set(u => u.ResetToken, token)
            .Set(u => u.ResetTokenExpiry, expiry);
        _context.Users.UpdateOne(filter, update);

        // 4. Send email using Mailtrap
        var resetLink = $"http://localhost:5173/reset-password?token={token}";

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("no-reply@example.com"));
        email.To.Add(MailboxAddress.Parse(request.Email));
        email.Subject = "Password Reset Request";
        email.Body = new TextPart("plain")
        {
            Text = $"You requested a password reset.\nClick this link to reset your password:\n{resetLink}"
        };

        using var smtp = new SmtpClient();
        smtp.Connect("sandbox.smtp.mailtrap.io", 587, MailKit.Security.SecureSocketOptions.StartTls);

        // Replace with your actual Mailtrap SMTP credentials
        smtp.Authenticate("d759da824c044c", "9233fb3dc20b92");

        smtp.Send(email);
        smtp.Disconnect(true);

        return Ok(new
        {
            message = "Reset password link has been sent to your email.",
            token = token // remove this line in production
        });
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.ResetToken, req.Token),
            Builders<User>.Filter.Gt(u => u.ResetTokenExpiry, DateTime.UtcNow)
        );

        var user = _context.Users.Find(filter).FirstOrDefault();

        if (user == null)
            return BadRequest(new { message = "Invalid or expired token." });

        // Hash password
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

        var update = Builders<User>.Update
            .Set(u => u.Password, hashedPassword)
            .Set(u => u.ResetToken, null)
            .Set(u => u.ResetTokenExpiry, null);

        _context.Users.UpdateOne(
            Builders<User>.Filter.Eq(u => u.ResetToken, req.Token),
            update);

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
        var filter = Builders<User>.Filter.Eq(u => u.PhoneNumber, request.PhoneNumber);
        var user = _context.Users.Find(filter).FirstOrDefault();

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Generate OTP token
        var token = new Random().Next(100000, 999999).ToString(); // 6-digit code
        var expiry = DateTime.UtcNow.AddMinutes(5);

        var update = Builders<User>.Update
            .Set(u => u.ResetToken, token)
            .Set(u => u.ResetTokenExpiry, expiry);
        _context.Users.UpdateOne(filter, update);

        // Send SMS using Twilio
        var accountSid = "AC58b121fb266609475e502722eaad7390";
        var authToken = "45c04b1a1ceb4fcb718bb27eb5308b5d";
        var twilioPhone = "+1234567890";

        TwilioClient.Init(accountSid, authToken);

        var phone = request.PhoneNumber;
        if (!phone.StartsWith("+"))
        {
            phone = "+91" + phone;
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
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.PhoneNumber, req.PhoneNumber),
            Builders<User>.Filter.Eq(u => u.ResetToken, req.Token),
            Builders<User>.Filter.Gt(u => u.ResetTokenExpiry, DateTime.UtcNow)
        );

        var user = _context.Users.Find(filter).FirstOrDefault();

        if (user == null)
            return BadRequest(new { message = "Invalid or expired token." });

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

        var update = Builders<User>.Update
            .Set(u => u.Password, hashedPassword)
            .Set(u => u.ResetToken, null)
            .Set(u => u.ResetTokenExpiry, null);

        _context.Users.UpdateOne(
            Builders<User>.Filter.Eq(u => u.PhoneNumber, req.PhoneNumber),
            update);

        return Ok(new { message = "Password reset successful." });
    }

    public class VerifyTokenRequest
    {
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}