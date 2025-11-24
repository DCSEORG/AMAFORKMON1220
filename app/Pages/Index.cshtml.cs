using Microsoft.AspNetCore.Mvc.RazorPages;
using app.Services;
using app.Models;

namespace app.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<IndexModel> _logger;

        public List<Expense> Expenses { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? ErrorLocation { get; set; }

        public IndexModel(DatabaseService dbService, ILogger<IndexModel> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Expenses = await _dbService.GetExpensesAsync();
            
            if (!string.IsNullOrEmpty(_dbService.LastError))
            {
                ErrorMessage = _dbService.LastError;
                ErrorLocation = _dbService.LastErrorLocation;
            }
        }
    }
}
