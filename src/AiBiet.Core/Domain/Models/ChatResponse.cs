namespace AiBiet.Core.Domain.Models;

public class ChatResponse
{
    public string Content { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public TokenUsage? Usage { get; set; }

    public static ChatResponse Success(string content, string model, TokenUsage? usage = null)
    {
        return new ChatResponse
        {
            Content = content,
            Model = model,
            Usage = usage,
            IsSuccess = true
        };
    }

    public static ChatResponse Failure(string errorMessage)
    {
        return new ChatResponse
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

public record TokenUsage(int PromptTokens, int CompletionTokens)
{
    public int TotalTokens => PromptTokens + CompletionTokens;
}
