namespace Velum.Core.Interfaces;

public interface IOpenAIService
{
    Task<string> GetChatCompletionAsync(string prompt);
    Task<string> ParseQuestionnaireFromTextAsync(string text);
}
