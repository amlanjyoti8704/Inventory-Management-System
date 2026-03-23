using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly MongoDbContext _context;

    public CategoriesController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetCategories([FromQuery] string search = "", [FromQuery] string sortBy = "category_id", [FromQuery] string sortOrder = "asc", [FromQuery] int? thresholdMin = null)
    {
        try
        {
            var filterBuilder = Builders<Category>.Filter;
            var filters = new List<FilterDefinition<Category>>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filters.Add(filterBuilder.Regex(c => c.CategoryName, new MongoDB.Bson.BsonRegularExpression(search, "i")));
            }

            if (thresholdMin.HasValue)
            {
                filters.Add(filterBuilder.Gte(c => c.Threshold, thresholdMin.Value));
            }

            var filter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            // Build sort definition
            SortDefinition<Category> sort;
            var sortBuilder = Builders<Category>.Sort;

            sort = sortBy switch
            {
                "category_name" => sortOrder.ToLower() == "desc"
                    ? sortBuilder.Descending(c => c.CategoryName)
                    : sortBuilder.Ascending(c => c.CategoryName),
                "threshold" => sortOrder.ToLower() == "desc"
                    ? sortBuilder.Descending(c => c.Threshold)
                    : sortBuilder.Ascending(c => c.Threshold),
                _ => sortOrder.ToLower() == "desc"
                    ? sortBuilder.Descending(c => c.CategoryId)
                    : sortBuilder.Ascending(c => c.CategoryId)
            };

            var categories = _context.Categories
                .Find(filter)
                .Sort(sort)
                .ToList();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving categories", error = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult AddCategory([FromBody] Category category)
    {
        try
        {
            category.CategoryId = _context.GetNextSequence("category");

            _context.Categories.InsertOne(category);

            return Ok(new { message = "Category added successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error adding category", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public IActionResult UpdateCategory(int id, [FromBody] Category category)
    {
        try
        {
            var filter = Builders<Category>.Filter.Eq(c => c.CategoryId, id);
            var update = Builders<Category>.Update
                .Set(c => c.CategoryName, category.CategoryName)
                .Set(c => c.Threshold, category.Threshold);

            var result = _context.Categories.UpdateOne(filter, update);

            if (result.ModifiedCount > 0)
                return Ok(new { message = "Category updated successfully!" });
            else
                return NotFound(new { message = "Category not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating category", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteCategory(int id)
    {
        try
        {
            var filter = Builders<Category>.Filter.Eq(c => c.CategoryId, id);
            var result = _context.Categories.DeleteOne(filter);

            if (result.DeletedCount > 0)
                return Ok(new { message = "Category deleted successfully!" });
            else
                return NotFound(new { message = "Category not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting category", error = ex.Message });
        }
    }
}
