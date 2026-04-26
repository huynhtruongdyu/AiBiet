using System.IO.Compression;
using System.Reflection;

using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;

namespace AiBiet.CLI.Infrastructure;

internal class ToolScanner : IToolScanner
{
    public async Task<IEnumerable<ToolInfo>> ScanAsync(IEnumerable<string> sources, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var tools = new List<ToolInfo>();

        foreach (var source in sources)
        {
            if (!Directory.Exists(source))
            {
                continue;
            }

            var toolPackageFiles = Directory.GetFiles(source, "*.nupkg", SearchOption.AllDirectories);

            foreach (var packageFile in toolPackageFiles)
            {
                try
                {
                    var packageTools = ScanPackage(packageFile);
                    tools.AddRange(packageTools);
                }
                catch
                {
                    // Skip packages that cannot be processed
                }
            }
        }

        return await Task.FromResult<IEnumerable<ToolInfo>>(tools).ConfigureAwait(false);
    }

    public async Task<ToolInfo?> FindToolAsync(string toolName, IEnumerable<string> sources, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources);

        foreach (var source in sources)
        {
            if (!Directory.Exists(source))
            {
                continue;
            }

            var toolPackageFiles = Directory.GetFiles(source, "*.nupkg", SearchOption.AllDirectories);

            foreach (var packageFile in toolPackageFiles)
            {
                try
                {
                    var packageTools = ScanPackage(packageFile);
                    var found = packageTools.FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase));
                    if (found != null)
                    {
                        return found;
                    }
                }
                catch
                {
                    // Skip packages that cannot be processed
                }
            }
        }

        return await Task.FromResult<ToolInfo?>(null).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ToolRegistrationInfo>> GetToolRegistrationsAsync(IEnumerable<string> sources, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var registrations = new List<ToolRegistrationInfo>();

        foreach (var source in sources)
        {
            if (!Directory.Exists(source))
            {
                continue;
            }

            var toolPackageFiles = Directory.GetFiles(source, "*.nupkg", SearchOption.AllDirectories);

            foreach (var packageFile in toolPackageFiles)
            {
                try
                {
                    registrations.AddRange(ScanPackageForRegistrations(packageFile));
                }
                catch
                {
                    // Skip packages that cannot be processed
                }
            }
        }

        return await Task.FromResult<IEnumerable<ToolRegistrationInfo>>(registrations).ConfigureAwait(false);
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
                    var assembly = Assembly.LoadFrom(dllFile);
                    var toolTypes = assembly.GetTypes()
                        .Where(t => t is { IsClass: true, IsAbstract: false } &&
                                    t.GetInterface("AiBiet.Core.Interfaces.ITool`1") != null);

                    foreach (var toolType in toolTypes)
                    {
                        var toolInterface = toolType.GetInterface("AiBiet.Core.Interfaces.ITool`1");
                        if (toolInterface != null)
                        {
                            var nameProp = toolInterface.GetProperty("Name");
                            var descProp = toolInterface.GetProperty("Description");

                            var toolInstance = Activator.CreateInstance(toolType);
                            var name = nameProp?.GetValue(toolInstance) as string ?? toolType.Name;
                            var description = descProp?.GetValue(toolInstance) as string;

                            tools.Add(new ToolInfo
                            {
                                Name = name,
                                Description = description,
                                Source = Path.GetFileName(packageFile),
                                PackagePath = packageFile
                            });
                        }
                    }
                }
                catch
                {
                    // Skip assemblies that cannot be loaded
                }
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
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
                    var assembly = Assembly.LoadFrom(dllFile);
                    var toolTypes = assembly.GetTypes()
                        .Where(t => t is { IsClass: true, IsAbstract: false } &&
                                    t.GetInterface("AiBiet.Core.Interfaces.ITool`1") != null);

                    foreach (var toolType in toolTypes)
                    {
                        var toolInterface = toolType.GetInterface("AiBiet.Core.Interfaces.ITool`1");
                        if (toolInterface != null)
                        {
                            var settingsType = toolInterface.GetGenericArguments()[0];
                            var nameProp = toolInterface.GetProperty("Name");
                            var descProp = toolInterface.GetProperty("Description");

                            var toolInstance = Activator.CreateInstance(toolType);
                            var name = nameProp?.GetValue(toolInstance) as string ?? toolType.Name;
                            var description = descProp?.GetValue(toolInstance) as string ?? "";

                            registrations.Add(new ToolRegistrationInfo
                            {
                                Name = name,
                                Description = description,
                                ToolType = toolType,
                                SettingsType = settingsType
                            });
                        }
                    }
                }
                catch
                {
                    // Skip assemblies that cannot be loaded
                }
            }
        }
        catch
        {
            // Skip packages that cannot be processed
        }
        // Note: We don't delete tempDir here because the loaded types might need the assembly file
        // on disk for some operations later (e.g. reflection on attributes).
        // For a CLI app, these temp dirs will be cleaned up by the OS eventually.

        return registrations;
    }
}
