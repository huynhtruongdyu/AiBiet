namespace AiBiet.Core.Domain.Models;

public class ToolRegistrationInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Type ToolType { get; set; } = null!;
    public Type SettingsType { get; set; } = null!;
}
