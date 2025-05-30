using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly DbContext _context = new DbContext();
    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("signup")]
    public IActionResult Signup(User user)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        using var conn = _context.GetConnection();
        conn.Open();

        var cmd = new MySqlCommand(
            "INSERT INTO users (email, username, password, role, createdAt) VALUES (@email, @username, @password, @role, @createdAt)",
            conn);

        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@password", user.Password);
        cmd.Parameters.AddWithValue("@role", string.IsNullOrEmpty(user.Role) ? "user" : user.Role);
        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);

        cmd.ExecuteNonQuery();

        // Fetch the inserted user (including ID)
        var getUserCmd = new MySqlCommand("SELECT id, username, email, role FROM users WHERE email = @Email", conn);
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

        var cmd = new MySqlCommand("SELECT * FROM users WHERE email = @Email", conn);
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

        var cmd = new MySqlCommand("SELECT id, username, email, role FROM users", conn);
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

        var query = "UPDATE users SET role = @Role WHERE email = @Email";
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

        var cmd = new MySqlCommand("SELECT id, username, email, role FROM users WHERE email = @Email", conn);
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
}