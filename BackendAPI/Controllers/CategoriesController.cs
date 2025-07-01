
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public CategoriesController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetCategories([FromQuery] string search = "", [FromQuery] string sortBy = "category_id", [FromQuery] string sortOrder = "asc", [FromQuery] int? thresholdMin = null)
    {
        List<Category> categories = new List<Category>();

        using (MySqlConnection conn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            try
            {
                conn.Open();

                // Build dynamic SQL query
                string query = "SELECT category_id, category_name, threshold FROM category WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " AND category_name LIKE @Search";
                }

                if (thresholdMin.HasValue)
                {
                    query += " AND threshold >= @ThresholdMin";
                }

                // Sanitize sortBy and sortOrder
                string safeSortBy = sortBy switch
                {
                    "category_id" => "category_id",
                    "category_name" => "category_name",
                    "threshold" => "threshold",
                    _ => "category_id"
                };

                string safeSortOrder = sortOrder.ToLower() == "desc" ? "DESC" : "ASC";

                query += $" ORDER BY {safeSortBy} {safeSortOrder}";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                        cmd.Parameters.AddWithValue("@Search", $"%{search}%");

                    if (thresholdMin.HasValue)
                        cmd.Parameters.AddWithValue("@ThresholdMin", thresholdMin.Value);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new Category
                            {
                                CategoryId = reader.GetInt32("category_id"),
                                CategoryName = reader.GetString("category_name"),
                                Threshold = reader.GetInt32("threshold")
                            });
                        }
                    }
                }

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving categories", error = ex.Message });
            }
        }
    }

    [HttpPost]
    public IActionResult AddCategory([FromBody] Category category)
    {
        using (MySqlConnection conn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            try
            {
                conn.Open();
                string query = "INSERT INTO category (category_name, threshold) VALUES (@CategoryName, @Threshold)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    cmd.Parameters.AddWithValue("@Threshold", category.Threshold);

                    cmd.ExecuteNonQuery();
                }

                return Ok(new { message = "Category added successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding category", error = ex.Message });
            }
        }
    }

    [HttpPut("{id}")]
    public IActionResult UpdateCategory(int id, [FromBody] Category category)
    {
        using (MySqlConnection conn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            try
            {
                conn.Open();
                string query = "UPDATE category SET category_name = @CategoryName, threshold = @Threshold WHERE category_id = @CategoryId";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryId", id);
                    cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    cmd.Parameters.AddWithValue("@Threshold", category.Threshold);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        return Ok(new { message = "Category updated successfully!" });
                    else
                        return NotFound(new { message = "Category not found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating category", error = ex.Message });
            }
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteCategory(int id)
    {
        using (MySqlConnection conn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM category WHERE category_id = @CategoryId";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryId", id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        return Ok(new { message = "Category deleted successfully!" });
                    else
                        return NotFound(new { message = "Category not found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting category", error = ex.Message });
            }
        }
    }
}
