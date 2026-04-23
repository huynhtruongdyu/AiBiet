namespace AiBiet.Core.Domain.Models;

public class ChatResponse
{
    public string Content { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    
    // Additional properties like token usage, stop reasons, etc. can be added here
}
