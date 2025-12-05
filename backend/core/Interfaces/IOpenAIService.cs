namespace Velum.Core.Interfaces;

public interface IOpenAIService
{
    Task<string> GetChatCompletionAsync(string prompt);
    IAsyncEnumerable<string> GetChatStreamingAsync(string prompt);
    Task<string> ParseQuestionnaireFromTextAsync(string text);
    Task<string> ParseAndTranslateQuestionnaireAsync(string text);
}
