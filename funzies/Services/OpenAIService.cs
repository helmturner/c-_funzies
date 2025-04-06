using OpenAI.Chat;

namespace funzies.Services;

public class OpenAIService : IOpenAIService
{
    private readonly ILogger<OpenAIService> _logger;
    private readonly ChatClient _client;

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        var _configuration = configuration;
        _logger = logger;

        string apiKey =
            _configuration["OpenAI:ApiKey"]
            ?? throw new ArgumentNullException(
                nameof(configuration),
                "OpenAI API key is not configured."
            );

        _client = new ChatClient(
            _configuration["OpenAI:Model"] ?? "gpt-4o-mini",
            new System.ClientModel.ApiKeyCredential(apiKey)
        );
    }

    public async Task<string> GenerateChatCompletionAsync(string message)
    {
        try
        {
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
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error generating chat completion for message: {Message}",
                message
            );
            return "I'm sorry, I couldn't process your request.";
        }
    }
}
