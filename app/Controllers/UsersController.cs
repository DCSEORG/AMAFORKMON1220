using Microsoft.AspNetCore.Mvc;
using app.Services;
using app.Models;

namespace app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(DatabaseService dbService, ILogger<UsersController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                var users = await _dbService.GetUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { error = "An error occurred while retrieving users" });
            }
        }
    }
}
