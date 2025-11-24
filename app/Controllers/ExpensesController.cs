using Microsoft.AspNetCore.Mvc;
using app.Services;
using app.Models;

namespace app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<ExpensesController> _logger;

        public ExpensesController(DatabaseService dbService, ILogger<ExpensesController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        /// <summary>
        /// Get all expenses
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
        {
            try
            {
                var expenses = await _dbService.GetExpensesAsync();
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expenses");
                return StatusCode(500, new { error = "An error occurred while retrieving expenses" });
            }
        }

        /// <summary>
        /// Get expense by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
            try
            {
                var expenses = await _dbService.GetExpensesAsync();
                var expense = expenses.FirstOrDefault(e => e.ExpenseId == id);
                
                if (expense == null)
                {
                    return NotFound();
                }
                
                return Ok(expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expense {ExpenseId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the expense" });
            }
        }
    }
}
