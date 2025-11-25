using System.ClientModel;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

using Velum.Core.Interfaces;

namespace Velum.Base.Services;

public class OpenAIService : IOpenAIService
{
    private readonly ChatClient _chatClient;

    public OpenAIService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        var baseUrl = configuration["OpenAI:BaseUrl"];
        var model = configuration["OpenAI:Model"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("OpenAI:ApiKey is required in configuration.");
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("OpenAI:Model is required in configuration.");

        OpenAIClientOptions options = new();
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            options.Endpoint = new Uri(baseUrl);
        }

        var client = new OpenAIClient(new ApiKeyCredential(apiKey), options);
        _chatClient = client.GetChatClient(model);
    }

    public async Task<string> GetChatCompletionAsync(string prompt)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            [
                new UserChatMessage(prompt)
            ]);

        if (completion.Content != null && completion.Content.Count > 0)
        {
            return completion.Content[0].Text;
        }

        return string.Empty;
    }

    public async Task<string> ParseQuestionnaireFromTextAsync(string text)
    {
        var systemPrompt = """
            You are an expert in psychological assessment.
            Convert the provided text into a structured JSON questionnaire format.
            The JSON should match this structure:
            {
                "Title": "string",
                "Description": "string",
                "Questions": [
                    {
                        "Id": int,
                        "Text": "string",
                        "Type": "SingleChoice" | "MultipleChoice" | "Text" | "Scale",
                        "Options": [
                            { "Text": "string", "Score": int }
                        ]
                    }
                ]
            }
            If the text implies scoring (e.g. "Never=0, Always=5"), apply it to the options.
            Only return the valid JSON string, do not wrap it in markdown code blocks.
            """;

        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(text)
            ]);

        if (completion.Content != null && completion.Content.Count > 0)
        {
            var result = completion.Content[0].Text;
            // Clean up potential markdown formatting if the model ignores the instruction
            if (result.StartsWith("```json"))
            {
                result = result.Substring(7);
            }
            if (result.StartsWith("```"))
            {
                result = result.Substring(3);
            }
            if (result.EndsWith("```"))
            {
                result = result.Substring(0, result.Length - 3);
            }
            return result.Trim();
        }

        return "{}";
    }
}
