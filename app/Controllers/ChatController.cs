using Microsoft.AspNetCore.Mvc;
using app.Services;

namespace app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly OpenAIService _openAIService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(OpenAIService openAIService, ILogger<ChatController> logger)
        {
            _openAIService = openAIService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request)
        {
            try
            {
                var response = await _openAIService.GetChatResponseAsync(request.Message);
                return Ok(new ChatResponse { Response = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, new ChatResponse 
                { 
                    Response = "Sorry, there was an error processing your request." 
                });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ChatResponse
    {
        public string Response { get; set; } = string.Empty;
    }
}
