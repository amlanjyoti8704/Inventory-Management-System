using Microsoft.AspNetCore.Mvc;
// using MySql.Data.MySqlClient;
using MySqlConnector;
using System.Data;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly DbContext _context = new DbContext();

    [HttpPost("signup")]
    public IActionResult Signup(User user)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        using var conn = _context.GetConnection();
        conn.Open();

        var cmd = new MySqlCommand("INSERT INTO users (email, username, password) VALUES (@email, @username, @password)", conn);
        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@password", user.Password);

        cmd.ExecuteNonQuery();
        return Ok(new { message = "User created" });
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest login)
    {
        using var conn = _context.GetConnection();
        conn.Open();

        var cmd = new MySqlCommand("SELECT * FROM users WHERE email = @email", conn);
        cmd.Parameters.AddWithValue("@email", login.Email);

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            string storedHash = reader["password"].ToString();
            if (BCrypt.Net.BCrypt.Verify(login.Password, storedHash))
            {
                return Ok(new { message = "Login successful", user = reader["username"] });
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

        var cmd = new MySqlCommand("SELECT * FROM users", conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            users.Add(new User
            {
                Username = reader["username"].ToString(),
                Email = reader["email"].ToString(),
                Password = reader["password"].ToString() // ⚠️ Consider omitting in real apps
            });
        }

        return Ok(users);
    }
}