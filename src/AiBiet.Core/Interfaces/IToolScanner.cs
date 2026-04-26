using AiBiet.Core.Domain.Models;

namespace AiBiet.Core.Interfaces;

public interface IToolScanner
{
    Task<IEnumerable<ToolInfo>> ScanAsync(IEnumerable<string> sources, CancellationToken cancellationToken = default);

    Task<ToolInfo?> FindToolAsync(string toolName, IEnumerable<string> sources, CancellationToken cancellationToken = default);

    Task<IEnumerable<ToolRegistrationInfo>> GetToolRegistrationsAsync(IEnumerable<string> sources, CancellationToken cancellationToken = default);
}
