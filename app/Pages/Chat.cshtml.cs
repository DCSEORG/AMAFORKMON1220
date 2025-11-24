using Microsoft.AspNetCore.Mvc.RazorPages;
using app.Services;

namespace app.Pages
{
    public class ChatModel : PageModel
    {
        private readonly OpenAIService _openAIService;
        public bool IsGenAIConfigured { get; set; }

        public ChatModel(OpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        public void OnGet()
        {
            IsGenAIConfigured = _openAIService.IsConfigured;
        }
    }
}
