using System.ClientModel;
using OpenAI.Chat;

namespace funzies.Services;

public class OpenAIService : IOpenAIService
{
    private readonly ApiKeyCredential _apiKey;

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        var _configuration = configuration;
        var _logger = logger;

        string? apiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("OpenAI API key is not configured.");
            throw new ArgumentNullException(
                nameof(configuration),
                "OpenAI API key is not configured."
            );
        }

        _apiKey = new ApiKeyCredential(apiKey);
    }

    public async Task<string> GenerateChatCompletionAsync(string message)
    {
        var _client = new ChatClient("gpt-4o-mini", _apiKey);
        var chatCompletionsOptions = new ChatCompletionOptions
        {
            Temperature = 0.7f,
            TopP = 1.0f,
            FrequencyPenalty = 0.0f,
            PresencePenalty = 0.0f,
            MaxOutputTokenCount = 1000,
        };

        var messages = new UserChatMessage[] { new UserChatMessage(message) };

        var response = await _client
            .CompleteChatAsync(messages, chatCompletionsOptions)
            .ConfigureAwait(false);

        var chatCompletions = response.Value;
        return chatCompletions.Content.ToArray()[0].Text ?? string.Empty;
    }
}
