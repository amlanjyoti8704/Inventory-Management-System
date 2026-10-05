using BackendAPI.Data;
using BackendAPI.Models;
using Npgsql;

namespace BackendAPI.Services
{
    /// <summary>
    /// Handles all user-related business logic: registration, authentication,
    /// password reset flows (email + SMS), role management.
    /// </summary>
    public class UserService
    {
        private readonly DbHelper _db;

        public UserService(DbHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// Registers a new user. Hashes the password with BCrypt before storing.
        /// Auto-generates the user ID via PostgreSQL SERIAL column.
        /// Defaults role to "user" if not provided.
        /// </summary>
        public async Task<User> SignupAsync(User user)
        {
            // Hash the plaintext password before persisting
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Role = string.IsNullOrEmpty(user.Role) ? "user" : user.Role;
            user.CreatedAt = DateTime.Now;

            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // RETURNING id fetches the auto-generated primary key
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO users (username, email, password, role, created_at)
                VALUES (@username, @email, @password, @role, @created_at)
                RETURNING id", conn);

            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Parameters.AddWithValue("email", user.Email);
            cmd.Parameters.AddWithValue("password", user.Password);
            cmd.Parameters.AddWithValue("role", user.Role);
            cmd.Parameters.AddWithValue("created_at", user.CreatedAt);

            // Capture the auto-generated ID
            user.Id = (int)(await cmd.ExecuteScalarAsync())!;
            return user;
        }

        /// <summary>
        /// Authenticates a user by email. Verifies the plaintext password
        /// against the stored BCrypt hash. Returns null if credentials are invalid.
        /// </summary>
        public async Task<User?> LoginAsync(string email, string password)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, username, email, password, role FROM users WHERE email = @email", conn);
            cmd.Parameters.AddWithValue("email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            var user = new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                Password = reader.GetString(3),
                Role = reader.GetString(4)
            };

            // BCrypt.Verify compares plaintext against stored hash
            return BCrypt.Net.BCrypt.Verify(password, user.Password) ? user : null;
        }

        /// <summary>
        /// Returns all users (excluding passwords) for admin user management screens.
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT id, username, email, role FROM users", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            var users = new List<User>();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    Role = reader.GetString(3)
                });
            }
            return users;
        }

        /// <summary>
        /// Updates a user's role (e.g., "user" → "admin"). Used by admin panel.
        /// Returns true if a row was actually modified.
        /// </summary>
        public async Task<bool> UpdateRoleAsync(string email, string role)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "UPDATE users SET role = @role WHERE email = @email", conn);
            cmd.Parameters.AddWithValue("role", role);
            cmd.Parameters.AddWithValue("email", email);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        /// <summary>
        /// Fetches a single user by email for the "me" / profile endpoint.
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, username, email, role FROM users WHERE email = @email", conn);
            cmd.Parameters.AddWithValue("email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                Role = reader.GetString(3)
            };
        }

        /// <summary>
        /// Generates a cryptographically secure reset token, stores it with
        /// a 1-hour expiry, and returns it. The caller (controller) handles
        /// sending the email.
        /// </summary>
        public async Task<(User? user, string? token)> GenerateResetTokenByEmailAsync(string email)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // Check if user exists
            using var checkCmd = new NpgsqlCommand(
                "SELECT id, username, email FROM users WHERE email = @email", conn);
            checkCmd.Parameters.AddWithValue("email", email);
            using var reader = await checkCmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return (null, null);

            var user = new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2)
            };
            await reader.CloseAsync();

            // Generate a 256-bit hex token (64 characters)
            var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            var expiry = DateTime.UtcNow.AddHours(1);

            // Persist token and expiry to DB
            using var updateCmd = new NpgsqlCommand(@"
                UPDATE users SET reset_token = @token, reset_token_expiry = @expiry
                WHERE email = @email", conn);
            updateCmd.Parameters.AddWithValue("token", token);
            updateCmd.Parameters.AddWithValue("expiry", expiry);
            updateCmd.Parameters.AddWithValue("email", email);
            await updateCmd.ExecuteNonQueryAsync();

            return (user, token);
        }

        /// <summary>
        /// Validates the reset token against the DB (must exist and not be expired),
        /// then updates the password and clears the token.
        /// </summary>
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // Find user with a valid (non-expired) token
            using var findCmd = new NpgsqlCommand(@"
                SELECT id FROM users
                WHERE reset_token = @token AND reset_token_expiry > @now", conn);
            findCmd.Parameters.AddWithValue("token", token);
            findCmd.Parameters.AddWithValue("now", DateTime.UtcNow);

            var userId = await findCmd.ExecuteScalarAsync();
            if (userId == null) return false;

            // Hash the new password and clear the token in one update
            var hashed = BCrypt.Net.BCrypt.HashPassword(newPassword);
            using var updateCmd = new NpgsqlCommand(@"
                UPDATE users SET password = @password, reset_token = NULL, reset_token_expiry = NULL
                WHERE id = @id", conn);
            updateCmd.Parameters.AddWithValue("password", hashed);
            updateCmd.Parameters.AddWithValue("id", (int)userId);
            await updateCmd.ExecuteNonQueryAsync();

            return true;
        }

        /// <summary>
        /// Generates a 6-digit OTP for phone-based password reset.
        /// Stores the OTP as the reset_token with a 5-minute expiry.
        /// Returns null if no user has the given phone number.
        /// </summary>
        public async Task<(User? user, string? token)> GenerateResetTokenByPhoneAsync(string phoneNumber)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var checkCmd = new NpgsqlCommand(
                "SELECT id, username, email FROM users WHERE phone_number = @phone", conn);
            checkCmd.Parameters.AddWithValue("phone", phoneNumber);
            using var reader = await checkCmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return (null, null);

            var user = new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2)
            };
            await reader.CloseAsync();

            // 6-digit numeric OTP (simpler than hex token for SMS)
            var token = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            using var updateCmd = new NpgsqlCommand(@"
                UPDATE users SET reset_token = @token, reset_token_expiry = @expiry
                WHERE phone_number = @phone", conn);
            updateCmd.Parameters.AddWithValue("token", token);
            updateCmd.Parameters.AddWithValue("expiry", expiry);
            updateCmd.Parameters.AddWithValue("phone", phoneNumber);
            await updateCmd.ExecuteNonQueryAsync();

            return (user, token);
        }

        /// <summary>
        /// Verifies a phone-based OTP and resets the password.
        /// Validates: phone + token match + token not expired.
        /// </summary>
        public async Task<bool> VerifyTokenAndResetAsync(string phoneNumber, string token, string newPassword)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            using var findCmd = new NpgsqlCommand(@"
                SELECT id FROM users
                WHERE phone_number = @phone AND reset_token = @token AND reset_token_expiry > @now", conn);
            findCmd.Parameters.AddWithValue("phone", phoneNumber);
            findCmd.Parameters.AddWithValue("token", token);
            findCmd.Parameters.AddWithValue("now", DateTime.UtcNow);

            var userId = await findCmd.ExecuteScalarAsync();
            if (userId == null) return false;

            var hashed = BCrypt.Net.BCrypt.HashPassword(newPassword);
            using var updateCmd = new NpgsqlCommand(@"
                UPDATE users SET password = @password, reset_token = NULL, reset_token_expiry = NULL
                WHERE id = @id", conn);
            updateCmd.Parameters.AddWithValue("password", hashed);
            updateCmd.Parameters.AddWithValue("id", (int)userId);
            await updateCmd.ExecuteNonQueryAsync();

            return true;
        }
    }
}
