using Microsoft.AspNetCore.Mvc;
using BackendAPI.Models;
using BackendAPI.Services;
using MailKit.Net.Smtp;
using MimeKit;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace BackendAPI.Controllers
{
    /// <summary>
    /// Handles user authentication, registration, profile, and password reset endpoints.
    /// All business logic is delegated to UserService.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public UserController(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        // POST api/user/signup — Register a new user
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] User user)
        {
            var created = await _userService.SignupAsync(user);
            return Ok(new
            {
                message = "User created",
                user = new { id = created.Id, username = created.Username, email = created.Email, role = created.Role }
            });
        }

        // POST api/user/login — Authenticate with email + password
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest login)
        {
            var user = await _userService.LoginAsync(login.Email, login.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(new
            {
                message = "Login successful",
                user = new { id = user.Id, username = user.Username, email = user.Email, role = user.Role }
            });
        }

        // GET api/user — List all users (admin)
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // PUT api/user/update-role — Change a user's role
        [HttpPut("update-role")]
        public async Task<IActionResult> UpdateUserRole([FromBody] RoleUpdateRequest request)
        {
            var updated = await _userService.UpdateRoleAsync(request.Email, request.Role);
            return updated
                ? Ok(new { message = "Role updated successfully" })
                : NotFound(new { message = "User not found" });
        }

        // GET api/user/me?email=... — Get current user profile
        [HttpGet("me")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new { id = user.Id, username = user.Username, email = user.Email, role = user.Role });
        }

        // POST api/user/forgot-password — Send password reset email via Mailtrap
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email is required" });

            var (user, token) = await _userService.GenerateResetTokenByEmailAsync(request.Email);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Send reset link via email (Mailtrap SMTP)
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
            smtp.Authenticate("d759da824c044c", "9233fb3dc20b92");
            smtp.Send(email);
            smtp.Disconnect(true);

            return Ok(new { message = "Reset password link has been sent to your email.", token });
        }

        // POST api/user/reset-password — Reset password using email token
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            var success = await _userService.ResetPasswordAsync(req.Token, req.NewPassword);
            return success
                ? Ok(new { message = "Password reset successful." })
                : BadRequest(new { message = "Invalid or expired token." });
        }

        // POST api/user/request-password-reset — Send OTP via SMS (Twilio)
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PhoneRequest request)
        {
            var (user, token) = await _userService.GenerateResetTokenByPhoneAsync(request.PhoneNumber);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Send OTP via Twilio SMS
            var accountSid = "AC58b121fb266609475e502722eaad7390";
            var authToken = "45c04b1a1ceb4fcb718bb27eb5308b5d";
            var twilioPhone = "+1234567890";

            TwilioClient.Init(accountSid, authToken);
            var phone = request.PhoneNumber.StartsWith("+") ? request.PhoneNumber : "+91" + request.PhoneNumber;

            MessageResource.Create(
                body: $"Your password reset code is {token}",
                from: new Twilio.Types.PhoneNumber(twilioPhone),
                to: new Twilio.Types.PhoneNumber(phone)
            );

            return Ok(new { message = "OTP sent to phone" });
        }

        // POST api/user/verify-token-reset — Verify phone OTP and reset password
        [HttpPost("verify-token-reset")]
        public async Task<IActionResult> VerifyTokenReset([FromBody] VerifyTokenRequest req)
        {
            var success = await _userService.VerifyTokenAndResetAsync(req.PhoneNumber, req.Token, req.NewPassword);
            return success
                ? Ok(new { message = "Password reset successful." })
                : BadRequest(new { message = "Invalid or expired token." });
        }
    }
}