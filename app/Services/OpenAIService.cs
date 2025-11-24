using Azure.AI.OpenAI;
using Azure.Identity;
using app.Models;
using System.Text.Json;

namespace app.Services
{
    public class OpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAIService> _logger;
        private readonly DatabaseService _dbService;
        private OpenAIClient? _client;
        private readonly string? _endpoint;
        private readonly string? _deploymentName;

        public bool IsConfigured { get; private set; }

        public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger, DatabaseService dbService)
        {
            _configuration = configuration;
            _logger = logger;
            _dbService = dbService;
            _endpoint = _configuration["OpenAI:Endpoint"];
            _deploymentName = _configuration["OpenAI:DeploymentName"];

            IsConfigured = !string.IsNullOrEmpty(_endpoint) && !string.IsNullOrEmpty(_deploymentName);

            if (IsConfigured)
            {
                try
                {
                    _client = new OpenAIClient(new Uri(_endpoint!), new DefaultAzureCredential());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to initialize OpenAI client");
                    IsConfigured = false;
                }
            }
        }

        public async Task<string> GetChatResponseAsync(string userMessage)
        {
            if (!IsConfigured || _client == null)
            {
                return "⚠️ GenAI services are not deployed. This is a demonstration response.\n\n" +
                       "To enable full AI-powered chat functionality, please deploy using the deploy-with-chat.sh script instead of deploy.sh.\n\n" +
                       "The full version will allow you to:\n" +
                       "• Query expense data using natural language\n" +
                       "• Get insights and analytics\n" +
                       "• Create and manage expenses through conversation\n\n" +
                       $"Your message was: \"{userMessage}\"";
            }

            try
            {
                var chatCompletionsOptions = new ChatCompletionsOptions
                {
                    DeploymentName = _deploymentName,
                    Messages =
                    {
                        new ChatRequestSystemMessage("You are a helpful assistant for an expense management system. " +
                            "You can help users query expenses, get insights, and manage their expense data. " +
                            "When users ask about expenses, you can call the available functions to retrieve data."),
                        new ChatRequestUserMessage(userMessage)
                    },
                    Tools =
                    {
                        new ChatCompletionsFunctionToolDefinition
                        {
                            Name = "get_expenses",
                            Description = "Retrieves all expenses from the database",
                            Parameters = BinaryData.FromObjectAsJson(new
                            {
                                type = "object",
                                properties = new { },
                                required = Array.Empty<string>()
                            })
                        },
                        new ChatCompletionsFunctionToolDefinition
                        {
                            Name = "get_expense_summary",
                            Description = "Gets a summary of expenses including total amount and count by status",
                            Parameters = BinaryData.FromObjectAsJson(new
                            {
                                type = "object",
                                properties = new { },
                                required = Array.Empty<string>()
                            })
                        }
                    }
                };

                var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
                var responseMessage = response.Value.Choices[0].Message;

                // Check if the model wants to call a function
                if (responseMessage.ToolCalls != null && responseMessage.ToolCalls.Count > 0)
                {
                    foreach (var toolCall in responseMessage.ToolCalls)
                    {
                        if (toolCall is ChatCompletionsFunctionToolCall functionCall)
                        {
                            string functionResult = await ExecuteFunctionAsync(functionCall.Name);

                            // Create a new chat with the function result
                            var followUpOptions = new ChatCompletionsOptions
                            {
                                DeploymentName = _deploymentName,
                                Messages =
                                {
                                    new ChatRequestSystemMessage("You are a helpful assistant for an expense management system."),
                                    new ChatRequestUserMessage(userMessage),
                                    new ChatRequestAssistantMessage(responseMessage),
                                    new ChatRequestToolMessage(functionResult, functionCall.Id)
                                }
                            };

                            var followUpResponse = await _client.GetChatCompletionsAsync(followUpOptions);
                            return followUpResponse.Value.Choices[0].Message.Content ?? "No response generated.";
                        }
                    }
                }

                return responseMessage.Content ?? "No response generated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat response");
                return $"Error: {ex.Message}";
            }
        }

        private async Task<string> ExecuteFunctionAsync(string functionName)
        {
            try
            {
                switch (functionName)
                {
                    case "get_expenses":
                        var expenses = await _dbService.GetExpensesAsync();
                        return JsonSerializer.Serialize(expenses.Select(e => new
                        {
                            e.ExpenseId,
                            e.UserName,
                            e.CategoryName,
                            e.Amount,
                            e.Description,
                            ExpenseDate = e.ExpenseDate.ToString("yyyy-MM-dd"),
                            e.StatusName
                        }));

                    case "get_expense_summary":
                        var allExpenses = await _dbService.GetExpensesAsync();
                        var summary = new
                        {
                            TotalExpenses = allExpenses.Count,
                            TotalAmount = allExpenses.Sum(e => e.Amount),
                            PendingCount = allExpenses.Count(e => e.StatusName == "Pending"),
                            ApprovedCount = allExpenses.Count(e => e.StatusName == "Approved"),
                            RejectedCount = allExpenses.Count(e => e.StatusName == "Rejected")
                        };
                        return JsonSerializer.Serialize(summary);

                    default:
                        return $"Function {functionName} not found";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing function {FunctionName}", functionName);
                return $"Error executing function: {ex.Message}";
            }
        }
    }
}
