using System.IO.Compression;
using System.Reflection;

using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;

using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace AiBiet.Infrastructure;

public class ToolManager : IToolManager
{
    private readonly AiBietConfig _config;
    private readonly ISettings _nugetSettings;

    public ToolManager(AiBietConfig config)
    {
        _config = config;
        _nugetSettings = Settings.LoadDefaultSettings(Environment.CurrentDirectory);
    }

    private static bool IsRemoteSource(string source)
    {
        return Uri.TryCreate(source, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    public async Task<IEnumerable<ToolInfo>> ListAvailableToolsAsync(CancellationToken cancellationToken = default)
    {
        return await ScanSourcesAsync(_config.ToolSources, "", cancellationToken).ConfigureAwait(false);
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
        var tool = await FindToolAsync(toolName, otherSources, cancellationToken).ConfigureAwait(false);

        if (tool == null) return false;

        if (IsRemoteSource(tool.Source))
        {
            return await DownloadRemoteToolAsync(tool.Source, tool.Name, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var fileName = Path.GetFileName(tool.PackagePath);
            var destination = Path.Combine(_config.ToolsPath, fileName);
            File.Copy(tool.PackagePath, destination, true);
            return true;
        }
    }

    public async Task<bool> RemoveToolAsync(string toolName, CancellationToken cancellationToken = default)
    {
        var tool = await FindToolAsync(toolName, [_config.ToolsPath], cancellationToken).ConfigureAwait(false);
        if (tool == null || !File.Exists(tool.PackagePath)) return false;

        File.Delete(tool.PackagePath);
        return true;
    }

    public async Task<bool> UpdateToolAsync(string toolName, CancellationToken cancellationToken = default)
    {
        // Check if installed
        var installedTool = await FindToolAsync(toolName, [_config.ToolsPath], cancellationToken).ConfigureAwait(false);
        if (installedTool == null) return false;

        // Find update
        var otherSources = _config.ToolSources.Where(s => !string.Equals(s, _config.ToolsPath, StringComparison.OrdinalIgnoreCase));
        var tool = await FindToolAsync(toolName, otherSources, cancellationToken).ConfigureAwait(false);
        if (tool == null) return false;

        if (IsRemoteSource(tool.Source))
        {
            if (File.Exists(installedTool.PackagePath))
            {
                File.Delete(installedTool.PackagePath);
            }
            return await DownloadRemoteToolAsync(tool.Source, tool.Name, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var fileName = Path.GetFileName(tool.PackagePath);
            var destination = Path.Combine(_config.ToolsPath, fileName);

            if (!string.Equals(installedTool.PackagePath, destination, StringComparison.OrdinalIgnoreCase) && File.Exists(installedTool.PackagePath))
            {
                File.Delete(installedTool.PackagePath);
            }

            File.Copy(tool.PackagePath, destination, true);
            return true;
        }
    }

    private async Task<IEnumerable<ToolInfo>> ScanSourcesAsync(IEnumerable<string> sources, string searchTerm, CancellationToken cancellationToken)
    {
        var tools = new List<ToolInfo>();
        var packageSourceProvider = new PackageSourceProvider(_nugetSettings);
        var configuredSources = packageSourceProvider.LoadPackageSources().ToList();

        foreach (var source in sources)
        {
            var actualSource = source;
            // Check if source is a key in NuGet.Config
            var namedSource = configuredSources.FirstOrDefault(s => string.Equals(s.Name, source, StringComparison.OrdinalIgnoreCase));
            if (namedSource != null)
            {
                actualSource = namedSource.Source;
            }

            if (IsRemoteSource(actualSource))
            {
                tools.AddRange(await SearchRemotePackagesAsync(actualSource, string.IsNullOrEmpty(searchTerm) ? "AiBiet" : searchTerm, cancellationToken).ConfigureAwait(false));
            }
            else if (Directory.Exists(actualSource))
            {
                var packages = Directory.GetFiles(actualSource, "*.nupkg", SearchOption.AllDirectories);
                foreach (var package in packages)
                {
                    tools.AddRange(ScanPackage(package));
                }
            }
        }
        return tools;
    }

    private async Task<ToolInfo?> FindToolAsync(string toolName, IEnumerable<string> sources, CancellationToken cancellationToken)
    {
        foreach (var source in sources)
        {
            if (IsRemoteSource(source))
            {
                var results = await SearchRemotePackagesAsync(source, toolName, cancellationToken).ConfigureAwait(false);
                var exact = results.FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase));
                if (exact != null) return exact;

                var partial = results.FirstOrDefault(t => t.Name.Contains(toolName, StringComparison.OrdinalIgnoreCase));
                if (partial != null) return partial;
            }
            else if (Directory.Exists(source))
            {
                var packages = Directory.GetFiles(source, "*.nupkg", SearchOption.AllDirectories);
                foreach (var package in packages)
                {
                    var found = ScanPackage(package).FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase));
                    if (found != null) return found;
                }
            }
        }
        return null;
    }

    private SourceRepository CreateRepository(string source)
    {
        var packageSourceProvider = new PackageSourceProvider(_nugetSettings);
        var configuredSources = packageSourceProvider.LoadPackageSources();
        
        // Flexible matching for credentials
        var sourceUri = new Uri(source);
        var matchingSource = configuredSources.FirstOrDefault(s => 
            Uri.TryCreate(s.Source, UriKind.Absolute, out var uri) && 
            uri.GetLeftPart(UriPartial.Path).TrimEnd('/') == sourceUri.GetLeftPart(UriPartial.Path).TrimEnd('/'));

        var packageSource = new PackageSource(source);
        if (matchingSource != null)
        {
            packageSource.Credentials = matchingSource.Credentials;
        }

        var sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, Repository.Provider.GetCoreV3());
        return sourceRepositoryProvider.CreateRepository(packageSource);
    }

    private async Task<IEnumerable<ToolInfo>> SearchRemotePackagesAsync(string source, string searchTerm, CancellationToken cancellationToken)
    {
        var tools = new List<ToolInfo>();
        try
        {
            var sourceRepository = CreateRepository(source);
            var searchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>(cancellationToken).ConfigureAwait(false);
            if (searchResource != null)
            {
                var searchFilter = new SearchFilter(includePrerelease: true);
                var results = await searchResource.SearchAsync(
                    searchTerm,
                    searchFilter,
                    0,
                    20,
                    NullLogger.Instance,
                    cancellationToken).ConfigureAwait(false);

                foreach (var result in results)
                {
                    tools.Add(new ToolInfo
                    {
                        Name = result.Identity.Id,
                        Description = result.Description ?? result.Title,
                        Source = source,
                        PackagePath = ""
                    });
                }
            }
        }
        catch { /* Ignore remote errors */ }
        return tools;
    }

    private async Task<bool> DownloadRemoteToolAsync(string source, string packageId, CancellationToken cancellationToken)
    {
        try
        {
            var sourceRepository = CreateRepository(source);
            var resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken).ConfigureAwait(false);
            if (resource == null) return false;

            using var cacheContext = new SourceCacheContext();
            var versions = await resource.GetAllVersionsAsync(packageId, cacheContext, NullLogger.Instance, cancellationToken).ConfigureAwait(false);
            var latestVersion = versions.OrderByDescending(v => v).FirstOrDefault();

            if (latestVersion == null) return false;

            var fileName = $"{packageId}.{latestVersion.ToNormalizedString()}.nupkg";
            var destination = Path.Combine(_config.ToolsPath, fileName);

            using var fileStream = File.Create(destination);
            var success = await resource.CopyNupkgToStreamAsync(
                packageId,
                latestVersion,
                fileStream,
                cacheContext,
                NullLogger.Instance,
                cancellationToken).ConfigureAwait(false);

            return success;
        }
        catch
        {
            return false;
        }
    }

    private static IEnumerable<Type> GetToolTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes().Where(t => IsToolType(t));
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null && IsToolType(t))!;
        }
        catch
        {
            return [];
        }
    }

    private static bool IsToolType(Type type)
    {
        if (type is not { IsClass: true, IsAbstract: false }) return false;
        
        return type.GetInterfaces().Any(i => 
            i.IsGenericType && 
            (i.GetGenericTypeDefinition().FullName == "AiBiet.Core.Interfaces.ITool`1" || 
             i.GetGenericTypeDefinition().Name == "ITool`1"));
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
                    var toolTypes = GetToolTypes(assembly);

                    foreach (var toolType in toolTypes)
                    {
                        var toolInterface = toolType.GetInterfaces().First(i => i.IsGenericType && (i.GetGenericTypeDefinition().Name == "ITool`1" || i.GetGenericTypeDefinition().FullName == "AiBiet.Core.Interfaces.ITool`1"));
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
                    var toolTypes = GetToolTypes(assembly);

                    foreach (var toolType in toolTypes)
                    {
                        var toolInterface = toolType.GetInterfaces().First(i => i.IsGenericType && (i.GetGenericTypeDefinition().Name == "ITool`1" || i.GetGenericTypeDefinition().FullName == "AiBiet.Core.Interfaces.ITool`1"));
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
