using System.IO.Compression;
using System.Reflection;

using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;

using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace AiBiet.Infrastructure;

public class ToolManager(AiBietConfig config) : IToolManager
{
    private readonly AiBietConfig _config = config;
    private readonly ISettings _nugetSettings = Settings.LoadDefaultSettings(Environment.CurrentDirectory);

    public async Task<IEnumerable<ToolInfo>> ListAvailableToolsAsync(CancellationToken cancellationToken = default)
    {
        return await ScanSourcesAsync(_config.ToolSources, "", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ToolRegistrationInfo>> GetToolRegistrationsAsync(CancellationToken cancellationToken = default)
    {
        var registrations = new List<ToolRegistrationInfo>();

        if (Directory.Exists(_config.ToolsPath))
        {
            registrations.AddRange(ScanDirectoryForRegistrations(_config.ToolsPath));
        }

        return await Task.FromResult(registrations.DistinctBy(r => r.Name)).ConfigureAwait(false);
    }

    public async Task<bool> InstallToolAsync(string toolName, CancellationToken cancellationToken = default)
    {
        var source = _config.ToolSources.Where(s => !IsToolsPath(s));
        var tool = await FindToolAsync(toolName, source, cancellationToken).ConfigureAwait(false);

        if (tool == null) return false;

        if (IsRemoteSource(tool.Source))
        {
            return await DownloadRemoteToolAsync(tool.Source, tool.Name, cancellationToken).ConfigureAwait(false);
        }

        var destination = Path.Combine(_config.ToolsPath, Path.GetFileName(tool.PackagePath));
        File.Copy(tool.PackagePath, destination, true);
        return true;
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
        var installed = await FindToolAsync(toolName, [_config.ToolsPath], cancellationToken).ConfigureAwait(false);
        if (installed == null) return false;

        var source = _config.ToolSources.Where(s => !IsToolsPath(s));
        var tool = await FindToolAsync(toolName, source, cancellationToken).ConfigureAwait(false);
        if (tool == null) return false;

        if (IsRemoteSource(tool.Source))
        {
            if (File.Exists(installed.PackagePath)) File.Delete(installed.PackagePath);
            return await DownloadRemoteToolAsync(tool.Source, tool.Name, cancellationToken).ConfigureAwait(false);
        }

        var destination = Path.Combine(_config.ToolsPath, Path.GetFileName(tool.PackagePath));
        if (!string.Equals(installed.PackagePath, destination, StringComparison.OrdinalIgnoreCase) && File.Exists(installed.PackagePath))
        {
            File.Delete(installed.PackagePath);
        }

        File.Copy(tool.PackagePath, destination, true);
        return true;
    }

    private async Task<IEnumerable<ToolInfo>> ScanSourcesAsync(IEnumerable<string> sources, string searchTerm, CancellationToken cancellationToken)
    {
        var tools = new List<ToolInfo>();
        var packageSourceProvider = new PackageSourceProvider(_nugetSettings);
        var configuredSources = packageSourceProvider.LoadPackageSources().ToList();

        foreach (var source in sources)
        {
            var actualSource = configuredSources.FirstOrDefault(s => string.Equals(s.Name, source, StringComparison.OrdinalIgnoreCase))?.Source ?? source;

            if (IsRemoteSource(actualSource))
            {
                var query = string.IsNullOrEmpty(searchTerm) ? "AiBiet" : searchTerm;
                tools.AddRange(await SearchRemotePackagesAsync(actualSource, query, cancellationToken).ConfigureAwait(false));
            }
            else if (Directory.Exists(actualSource))
            {
                tools.AddRange(ScanDirectoryForInfo(actualSource));
            }
        }
        return tools;
    }

    private async Task<ToolInfo?> FindToolAsync(string toolName, IEnumerable<string> sources, CancellationToken cancellationToken)
    {
        var allFound = new List<ToolInfo>();
        foreach (var source in sources)
        {
            if (IsRemoteSource(source))
            {
                allFound.AddRange(await SearchRemotePackagesAsync(source, toolName, cancellationToken).ConfigureAwait(false));
            }
            else if (Directory.Exists(source))
            {
                allFound.AddRange(ScanDirectoryForInfo(source));
            }
        }

        // Prioritize: 1. Exact match, 2. Starts with AiBiet.Tools. + toolName, 3. Contains toolName
        return allFound.FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase))
               ?? allFound.FirstOrDefault(t => string.Equals(t.Name, $"AiBiet.Tools.{toolName}", StringComparison.OrdinalIgnoreCase))
               ?? allFound.FirstOrDefault(t => t.Name.Contains(toolName, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<ToolInfo> ScanDirectoryForInfo(string path)
    {
        var results = new List<ToolInfo>();
        var packages = Directory.GetFiles(path, "*.nupkg", SearchOption.AllDirectories);
        foreach (var pkg in packages) results.AddRange(ScanPackageForInfo(pkg));

        var dlls = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
        foreach (var dll in dlls) results.AddRange(DiscoverInAssembly(dll, (type, _, instance) => new ToolInfo
        {
            Name = GetProperty(instance, "Name") ?? type.Name,
            Description = GetProperty(instance, "Description"),
            Source = "Local DLL",
            PackagePath = dll
        }));

        return results;
    }

    private static IEnumerable<ToolRegistrationInfo> ScanDirectoryForRegistrations(string path)
    {
        var results = new List<ToolRegistrationInfo>();
        var packages = Directory.GetFiles(path, "*.nupkg", SearchOption.AllDirectories);
        foreach (var pkg in packages) results.AddRange(ScanPackageForRegistrations(pkg));

        var dlls = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
        foreach (var dll in dlls) results.AddRange(DiscoverInAssembly(dll, (type, settings, instance) => new ToolRegistrationInfo
        {
            Name = GetProperty(instance, "Name") ?? type.Name,
            Description = GetProperty(instance, "Description") ?? "",
            ToolType = type,
            SettingsType = settings
        }));

        return results;
    }

    private static IEnumerable<ToolInfo> ScanPackageForInfo(string packageFile)
    {
        return ScanPackageContents(packageFile, dll => DiscoverInAssembly(dll, (type, _, instance) => new ToolInfo
        {
            Name = GetProperty(instance, "Name") ?? type.Name,
            Description = GetProperty(instance, "Description"),
            Source = Path.GetFileName(packageFile),
            PackagePath = packageFile
        }));
    }

    private static IEnumerable<ToolRegistrationInfo> ScanPackageForRegistrations(string packageFile)
    {
        return ScanPackageContents(packageFile, dll => DiscoverInAssembly(dll, (type, settings, instance) => new ToolRegistrationInfo
        {
            Name = GetProperty(instance, "Name") ?? type.Name,
            Description = GetProperty(instance, "Description") ?? "",
            ToolType = type,
            SettingsType = settings
        }));
    }

    private static IEnumerable<T> ScanPackageContents<T>(string packageFile, Func<string, IEnumerable<T>> scanner)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"aibiet_{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(tempDir);
            ZipFile.ExtractToDirectory(packageFile, tempDir);
            return Directory.GetFiles(tempDir, "*.dll", SearchOption.AllDirectories).SelectMany(scanner).ToList();
        }
        catch { return []; }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); }
                catch { /* Ignore cleanup errors as this is a best-effort operation in a temp folder */ }
            }
        }
    }

    private static IEnumerable<T> DiscoverInAssembly<T>(string dllFile, Func<Type, Type, object, T> mapper)
    {
        var fileName = Path.GetFileName(dllFile);
        if (!fileName.StartsWith("AiBiet.Tools.", StringComparison.OrdinalIgnoreCase)) return [];

        try
        {
            var assembly = Assembly.Load(File.ReadAllBytes(dllFile));
            return GetToolTypes(assembly)
                .Where(IsToolType)
                .Select(type =>
                {
                    var toolInterface = type.GetInterfaces().First(i => i.IsGenericType && (i.GetGenericTypeDefinition().Name == "ITool`1" || i.GetGenericTypeDefinition().FullName == "AiBiet.Core.Interfaces.ITool`1"));
                    var instance = Activator.CreateInstance(type);
                    return mapper(type, toolInterface.GetGenericArguments()[0], instance!);
                })
                .ToList();
        }
        catch { return []; }
    }

    private static IEnumerable<Type> GetToolTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
        catch
        {
            return [];
        }
    }

    private static bool IsToolType(Type t) => t is { IsClass: true, IsAbstract: false } && t.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition().Name == "ITool`1" || i.GetGenericTypeDefinition().FullName == "AiBiet.Core.Interfaces.ITool`1"));

    private static string? GetProperty(object? obj, string name) => obj?.GetType().GetProperty(name)?.GetValue(obj) as string;

    private bool IsToolsPath(string path) => string.Equals(path, _config.ToolsPath, StringComparison.OrdinalIgnoreCase);

    private static bool IsRemoteSource(string s) => Uri.TryCreate(s, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private SourceRepository CreateRepository(string source)
    {
        var provider = new PackageSourceProvider(_nugetSettings);
        var sourceUri = new Uri(source);
        var matching = provider.LoadPackageSources().FirstOrDefault(s => Uri.TryCreate(s.Source, UriKind.Absolute, out var uri) && uri.GetLeftPart(UriPartial.Path).TrimEnd('/') == sourceUri.GetLeftPart(UriPartial.Path).TrimEnd('/'));

        var packageSource = new PackageSource(source);
        if (matching != null) packageSource.Credentials = matching.Credentials;

        return new SourceRepositoryProvider(provider, Repository.Provider.GetCoreV3()).CreateRepository(packageSource);
    }

    private async Task<IEnumerable<ToolInfo>> SearchRemotePackagesAsync(string source, string searchTerm, CancellationToken cancellationToken)
    {
        try
        {
            var repo = CreateRepository(source);
            var search = await repo.GetResourceAsync<PackageSearchResource>(cancellationToken).ConfigureAwait(false);
            if (search == null) return [];

            var results = await search.SearchAsync(searchTerm, new SearchFilter(true), 0, 20, NullLogger.Instance, cancellationToken).ConfigureAwait(false);
            return results.Select(r => new ToolInfo { Name = r.Identity.Id, Description = r.Description ?? r.Title, Source = source });
        }
        catch { return []; }
    }

    private async Task<bool> DownloadRemoteToolAsync(string source, string packageId, CancellationToken cancellationToken)
    {
        try
        {
            var repo = CreateRepository(source);
            var find = await repo.GetResourceAsync<FindPackageByIdResource>(cancellationToken).ConfigureAwait(false);
            if (find == null) return false;

            using var cache = new SourceCacheContext();
            var versions = await find.GetAllVersionsAsync(packageId, cache, NullLogger.Instance, cancellationToken).ConfigureAwait(false);
            var latest = versions.OrderByDescending(v => v).FirstOrDefault();
            if (latest == null) return false;

            var destination = Path.Combine(_config.ToolsPath, $"{packageId}.{latest.ToNormalizedString()}.nupkg");
            using var file = File.Create(destination);
            return await find.CopyNupkgToStreamAsync(packageId, latest, file, cache, NullLogger.Instance, cancellationToken).ConfigureAwait(false);
        }
        catch { return false; }
    }
}
