using Microsoft.AspNetCore.Mvc;
using app.Services;
using app.Models;

namespace app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(DatabaseService dbService, ILogger<CategoriesController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        /// <summary>
        /// Get all expense categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseCategory>>> GetCategories()
        {
            try
            {
                var categories = await _dbService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, new { error = "An error occurred while retrieving categories" });
            }
        }
    }
}
