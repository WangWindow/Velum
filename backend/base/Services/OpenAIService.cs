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

        OpenAIClientOptions options = new()
        {
            NetworkTimeout = TimeSpan.FromMinutes(5)
        };
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

    public async IAsyncEnumerable<string> GetChatStreamingAsync(string prompt)
    {
        AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = _chatClient.CompleteChatStreamingAsync(
            [
                new UserChatMessage(prompt)
            ]);

        await foreach (StreamingChatCompletionUpdate update in completionUpdates)
        {
            if (update.ContentUpdate.Count > 0)
            {
                var text = update.ContentUpdate[0].Text;
                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }
        }
    }

    public async Task<string> ParseQuestionnaireFromTextAsync(string text)
    {
        var systemPrompt = """
            You are an expert in psychological assessment and data structuring.
            Your task is to convert the provided raw text (which may be a psychological scale or questionnaire) into a strict, valid JSON format.

            The JSON must adhere to the following schema exactly:
            {
                "Title": "string (The name of the scale)",
                "Description": "string (A brief description or instructions)",
                "Questions": [
                    {
                        "Id": int (Sequential number starting from 1),
                        "Text": "string (The question text)",
                        "Type": "SingleChoice" | "MultipleChoice" | "Text" | "Scale",
                        "Options": [
                            { "Text": "string (Option label)", "Score": int (Score value) }
                        ]
                    }
                ]
            }

            Rules:
            1. Extract the Title and Description from the beginning of the text if available.
            2. Identify all questions and their options.
            3. If options have associated scores (e.g., "Not at all (0)", "A little (1)"), extract the score. If no score is explicit but implied (e.g., Likert scale), assign logical scores starting from 0 or 1.
            4. Determine the 'Type' based on the context. Most scales are 'SingleChoice'.
            5. RETURN ONLY THE RAW JSON STRING. Do not include markdown formatting (like ```json ... ```). Do not include any conversational text.
            6. Ensure the JSON is valid and can be parsed by a standard JSON parser.
            """;

        try
        {
            Console.WriteLine($"[AI Parse] Starting request for text length: {text.Length}");
            ChatCompletion completion = await _chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(text)
                ]);

            if (completion.Content != null && completion.Content.Count > 0)
            {
                var result = completion.Content[0].Text.Trim();

                // Debug output
                Console.WriteLine($"[AI Parse] Raw result length: {result.Length}");
                // Console.WriteLine($"[AI Parse] Preview: {result[..Math.Min(500, result.Length)]}");
                Console.WriteLine($"[AI Parse] Full Result: {result}"); // Show full result to debug truncation issues

                // Robust JSON extraction
                var firstBrace = result.IndexOf('{');
                var lastBrace = result.LastIndexOf('}');
                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    result = result.Substring(firstBrace, lastBrace - firstBrace + 1);
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AI Parse] Error: {ex.Message}");
            throw;
        }

        return "{}";
    }

    public async Task<string> ParseAndTranslateQuestionnaireAsync(string text)
    {
        var systemPrompt = """
            You are an expert in psychological assessment and translation.
            Your task is to:
            1. Parse the provided raw text into a structured JSON questionnaire.
            2. Translate the content into the OTHER language (if input is English, translate to Chinese; if Chinese, translate to English).
            3. Ensure both versions are culturally adapted and accurate.

            Return a JSON object with two keys: "en" and "zh".
            Each value must adhere to this schema:
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

            Rules:
            1. "en" key holds the English version.
            2. "zh" key holds the Chinese version.
            3. Ensure the structure (number of questions, IDs, scores) is identical between both versions.
            4. RETURN ONLY THE RAW JSON STRING. No markdown.
            """;

        try
        {
            Console.WriteLine($"[AI Bilingual Parse] Starting request for text length: {text.Length}");
            ChatCompletion completion = await _chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(text)
                ]);

            if (completion.Content != null && completion.Content.Count > 0)
            {
                var result = completion.Content[0].Text.Trim();

                // Debug output
                Console.WriteLine($"[AI Bilingual Parse] Raw result length: {result.Length}");
                // Console.WriteLine($"[AI Bilingual Parse] Preview: {result[..Math.Min(500, result.Length)]}");
                Console.WriteLine($"[AI Bilingual Parse] Full Result: {result}");

                // Robust JSON extraction
                var firstBrace = result.IndexOf('{');
                var lastBrace = result.LastIndexOf('}');
                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    result = result.Substring(firstBrace, lastBrace - firstBrace + 1);
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AI Bilingual Parse] Error: {ex.Message}");
            throw;
        }

        return "{}";
    }
}
