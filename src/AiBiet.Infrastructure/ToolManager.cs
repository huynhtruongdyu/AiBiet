using System.IO.Compression;
using System.Reflection;

using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;

namespace AiBiet.Infrastructure;

public class ToolManager(AiBietConfig config) : IToolManager
{
    private readonly AiBietConfig _config = config;

    public async Task<IEnumerable<ToolInfo>> ListAvailableToolsAsync(CancellationToken cancellationToken = default)
    {
        return await ScanSourcesAsync(_config.ToolSources).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ToolRegistrationInfo>> GetToolRegistrationsAsync(CancellationToken cancellationToken = default)
    {
        var registrations = new List<ToolRegistrationInfo>();
        var sources = new[] { _config.ToolsPath };

        foreach (var source in sources)
        {
            if (!Directory.Exists(source)) continue;

            var packages = Directory.GetFiles(source, "*.nupkg", SearchOption.AllDirectories);
            foreach (var package in packages)
            {
                registrations.AddRange(ScanPackageForRegistrations(package));
            }
        }

        return await Task.FromResult(registrations).ConfigureAwait(false);
    }

    public async Task<bool> InstallToolAsync(string toolName, CancellationToken cancellationToken = default)
    {
        var otherSources = _config.ToolSources.Where(s => !string.Equals(s, _config.ToolsPath, StringComparison.OrdinalIgnoreCase));
        var tool = await FindToolAsync(toolName, otherSources).ConfigureAwait(false);

        if (tool == null) return false;

        var fileName = Path.GetFileName(tool.PackagePath);
        var destination = Path.Combine(_config.ToolsPath, fileName);
        
        File.Copy(tool.PackagePath, destination, true);
        return true;
    }

    public async Task<bool> RemoveToolAsync(string toolName, CancellationToken cancellationToken = default)
    {
        var tool = await FindToolAsync(toolName, [_config.ToolsPath]).ConfigureAwait(false);
        if (tool == null || !File.Exists(tool.PackagePath)) return false;

        File.Delete(tool.PackagePath);
        return true;
    }

    public async Task<bool> UpdateToolAsync(string toolName, CancellationToken cancellationToken = default)
    {
        // Check if installed
        var installedTool = await FindToolAsync(toolName, [_config.ToolsPath]).ConfigureAwait(false);
        if (installedTool == null) return false;

        // Find update
        var otherSources = _config.ToolSources.Where(s => !string.Equals(s, _config.ToolsPath, StringComparison.OrdinalIgnoreCase));
        var tool = await FindToolAsync(toolName, otherSources).ConfigureAwait(false);
        if (tool == null) return false;

        var fileName = Path.GetFileName(tool.PackagePath);
        var destination = Path.Combine(_config.ToolsPath, fileName);

        if (!string.Equals(installedTool.PackagePath, destination, StringComparison.OrdinalIgnoreCase) && File.Exists(installedTool.PackagePath))
        {
            File.Delete(installedTool.PackagePath);
        }

        File.Copy(tool.PackagePath, destination, true);
        return true;
    }

    private static async Task<IEnumerable<ToolInfo>> ScanSourcesAsync(IEnumerable<string> sources)
    {
        var tools = new List<ToolInfo>();
        foreach (var source in sources)
        {
            if (!Directory.Exists(source)) continue;
            var packages = Directory.GetFiles(source, "*.nupkg", SearchOption.AllDirectories);
            foreach (var package in packages)
            {
                tools.AddRange(ScanPackage(package));
            }
        }
        return await Task.FromResult(tools).ConfigureAwait(false);
    }

    private static async Task<ToolInfo?> FindToolAsync(string toolName, IEnumerable<string> sources)
    {
        foreach (var source in sources)
        {
            if (!Directory.Exists(source)) continue;
            var packages = Directory.GetFiles(source, "*.nupkg", SearchOption.AllDirectories);
            foreach (var package in packages)
            {
                var found = ScanPackage(package).FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase));
                if (found != null) return found;
            }
        }
        return await Task.FromResult<ToolInfo?>(null).ConfigureAwait(false);
    }

    private static IEnumerable<ToolInfo> ScanPackage(string packageFile)
    {
        var tools = new List<ToolInfo>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"aibiet_scan_{Guid.NewGuid():N}");

        try
        {
            Directory.CreateDirectory(tempDir);
            ZipFile.ExtractToDirectory(packageFile, tempDir);
            var dllFiles = Directory.GetFiles(tempDir, "*.dll", SearchOption.AllDirectories);

            foreach (var dllFile in dllFiles)
            {
                try
                {
                    var assembly = Assembly.Load(File.ReadAllBytes(dllFile));
                    var toolTypes = assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterface("AiBiet.Core.Interfaces.ITool`1") != null);

                    foreach (var toolType in toolTypes)
                    {
                        var toolInterface = toolType.GetInterface("AiBiet.Core.Interfaces.ITool`1");
                        if (toolInterface != null)
                        {
                            var toolInstance = Activator.CreateInstance(toolType);
                            tools.Add(new ToolInfo
                            {
                                Name = toolInterface.GetProperty("Name")?.GetValue(toolInstance) as string ?? toolType.Name,
                                Description = toolInterface.GetProperty("Description")?.GetValue(toolInstance) as string,
                                Source = Path.GetFileName(packageFile),
                                PackagePath = packageFile
                            });
                        }
                    }
                }
                catch { /* Skip failed assemblies */ }
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { /* Ignore delete errors */ }
            }
        }

        return tools;
    }

    private static IEnumerable<ToolRegistrationInfo> ScanPackageForRegistrations(string packageFile)
    {
        var registrations = new List<ToolRegistrationInfo>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"aibiet_reg_{Guid.NewGuid():N}");

        try
        {
            Directory.CreateDirectory(tempDir);
            ZipFile.ExtractToDirectory(packageFile, tempDir);
            var dllFiles = Directory.GetFiles(tempDir, "*.dll", SearchOption.AllDirectories);

            foreach (var dllFile in dllFiles)
            {
                try
                {
                    var assembly = Assembly.Load(File.ReadAllBytes(dllFile));
                    var toolTypes = assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterface("AiBiet.Core.Interfaces.ITool`1") != null);

                    foreach (var toolType in toolTypes)
                    {
                        var toolInterface = toolType.GetInterface("AiBiet.Core.Interfaces.ITool`1");
                        if (toolInterface != null)
                        {
                            var toolInstance = Activator.CreateInstance(toolType);
                            registrations.Add(new ToolRegistrationInfo
                            {
                                Name = toolInterface.GetProperty("Name")?.GetValue(toolInstance) as string ?? toolType.Name,
                                Description = toolInterface.GetProperty("Description")?.GetValue(toolInstance) as string ?? "",
                                ToolType = toolType,
                                SettingsType = toolInterface.GetGenericArguments()[0]
                            });
                        }
                    }
                }
                catch { /* Skip failed assemblies */ }
            }
        }
        catch { /* Skip failed packages */ }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { /* Ignore delete errors */ }
            }
        }

        return registrations;
    }
}
