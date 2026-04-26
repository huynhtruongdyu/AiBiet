using System.Collections.ObjectModel;

namespace AiBiet.Core.Domain.Models;

public class ChatRequest
{
    public string? Model { get; set; }
    public Collection<ChatMessage> Messages { get; } = [];
    public float? Temperature { get; set; }
    public int? MaxTokens { get; set; }

    public static ChatRequest FromPrompt(string prompt, string? model = null)
    {
        var request = new ChatRequest
        {
            Model = model
        };
        request.Messages.Add(ChatMessage.User(prompt));
        return request;
    }
}
