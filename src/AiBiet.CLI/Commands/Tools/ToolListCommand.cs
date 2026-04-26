using System.ComponentModel;
using System.IO.Compression;
using System.Reflection;

using AiBiet.CLI.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands.Tools;

internal class ToolListSettings : CommandSettings
{
    [CommandOption("-o|--online")]
    [Description("Scan tools from configured sources")]
    public bool Online { get; set; }
}

internal class ToolListCommand : AsyncCommand<ToolListSettings>
{
    private readonly AiBietConfig _config;

    public ToolListCommand(AiBietConfig config)
    {
        _config = config;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ToolListSettings settings, CancellationToken cancellationToken)
    {
        if (!settings.Online)
        {
            AnsiConsole.MarkupLine("[yellow]Use --online to scan tools from configured sources.[/]");
            return 0;
        }

        var sources = _config.ToolSources;
        if (sources.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No tool sources configured.[/]");
            AnsiConsole.MarkupLine("Add sources using: [green]aibiet tools source add <path>[/]");
            return 0;
        }

        var tools = new List<ToolInfo>();

        foreach (var source in sources)
        {
            if (!Directory.Exists(source))
            {
                AnsiConsole.MarkupLine($"[yellow]Source not found: {source}[/]");
                continue;
            }

            var toolPackageFiles = Directory.GetFiles(source, "*.nupkg", SearchOption.AllDirectories);

            foreach (var packageFile in toolPackageFiles)
            {
#pragma warning disable CA1849
#pragma warning disable S6966
                try
                {
                    using var archive = ZipFile.OpenRead(packageFile);
                    var tempDir = Path.Combine(Path.GetTempPath(), $"aibiet_{Guid.NewGuid():N}");
                    Directory.CreateDirectory(tempDir);

                    try
                    {
                        archive.ExtractToDirectory(tempDir);

                        var dllFiles = Directory.GetFiles(tempDir, "*.dll", SearchOption.AllDirectories);

                        foreach (var dllFile in dllFiles)
                        {
                            try
                            {
                                var assembly = Assembly.LoadFrom(dllFile);
                                var toolTypes = assembly.GetTypes()
                                    .Where(t => t.IsClass && !t.IsAbstract && t.GetInterface("AiBiet.Core.Interfaces.ITool`1") != null);

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
                                            Source = Path.GetFileName(packageFile)
                                        });
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // Skip assemblies that cannot be loaded
                            }
                        }
                    }
                    finally
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch (Exception)
                {
                    // Skip packages that cannot be extracted
                }
#pragma warning restore CA1849
#pragma warning restore S6966
            }
        }

        if (tools.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No tools found.[/]");
            return 0;
        }

        var table = new Table();
        table.AddColumn("Name");
        table.AddColumn("Description");
        table.AddColumn("Source");

        foreach (var tool in tools)
        {
            table.AddRow(tool.Name, tool.Description ?? "", tool.Source);
        }

        AnsiConsole.Write(table);

        return await Task.FromResult(0).ConfigureAwait(false);
    }
}

internal class ToolInfo
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string Source { get; set; } = "";
}