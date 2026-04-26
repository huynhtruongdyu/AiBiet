using AiBiet.Core.Domain.Models;

namespace AiBiet.Core.Interfaces;

public interface IToolManager
{
    Task<IEnumerable<ToolInfo>> ListAvailableToolsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ToolRegistrationInfo>> GetToolRegistrationsAsync(CancellationToken cancellationToken = default);
    Task<bool> InstallToolAsync(string toolName, CancellationToken cancellationToken = default);
    Task<bool> RemoveToolAsync(string toolName, CancellationToken cancellationToken = default);
    Task<bool> UpdateToolAsync(string toolName, CancellationToken cancellationToken = default);
}
