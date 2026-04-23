namespace AiBiet.Core.Domain.Models;

public class ChatMessage
{
    public ChatRole Role { get; set; }
    public string Content { get; set; }

    public ChatMessage(ChatRole role, string content)
    {
        Role = role;
        Content = content ?? string.Empty;
    }
    
    public static ChatMessage System(string content) => new(ChatRole.System, content);
    public static ChatMessage User(string content) => new(ChatRole.User, content);
    public static ChatMessage Assistant(string content) => new(ChatRole.Assistant, content);
}
