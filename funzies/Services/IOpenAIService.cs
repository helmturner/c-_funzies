namespace funzies.Services;

public interface IOpenAIService
{
    Task<string> GenerateChatCompletionAsync(string message);
}
