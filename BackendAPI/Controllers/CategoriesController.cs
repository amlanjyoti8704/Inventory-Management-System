using Microsoft.AspNetCore.Mvc;
using BackendAPI.Models;
using BackendAPI.Services;

namespace BackendAPI.Controllers
{
    /// <summary>
    /// CRUD endpoints for inventory categories.
    /// Supports search, sorting, and threshold filtering.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoriesController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET api/categories?search=&sortBy=category_id&sortOrder=asc&thresholdMin=
        [HttpGet]
        public async Task<IActionResult> GetCategories(
            [FromQuery] string search = "",
            [FromQuery] string sortBy = "category_id",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] int? thresholdMin = null)
        {
            try
            {
                var categories = await _categoryService.GetAllAsync(search, sortBy, sortOrder, thresholdMin);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving categories", error = ex.Message });
            }
        }

        // POST api/categories — Add a new category
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            try
            {
                await _categoryService.AddAsync(category);
                return Ok(new { message = "Category added successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding category", error = ex.Message });
            }
        }

        // PUT api/categories/{id} — Update an existing category
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            try
            {
                var updated = await _categoryService.UpdateAsync(id, category);
                return updated
                    ? Ok(new { message = "Category updated successfully!" })
                    : NotFound(new { message = "Category not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating category", error = ex.Message });
            }
        }

        // DELETE api/categories/{id} — Delete a category
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var deleted = await _categoryService.DeleteAsync(id);
                return deleted
                    ? Ok(new { message = "Category deleted successfully!" })
                    : NotFound(new { message = "Category not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting category", error = ex.Message });
            }
        }
    }
}
