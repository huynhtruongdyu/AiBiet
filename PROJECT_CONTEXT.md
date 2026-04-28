# AiBiet Project Context

> **Purpose**: This document provides a comprehensive overview of the AiBiet project structure, architecture, and key concepts. Use this as a quick reference to understand the project without re-exploring all files.

## Project Overview

**AiBiet** (meaning "AI knows?" in Vietnamese) is a modular, AI-powered CLI assistant built with C# and .NET 10. It uses `Spectre.Console.Cli` for the command-line interface and supports multiple AI providers (currently Gemini is fully implemented).

**Key Facts:**
- **Language**: C# (.NET 10)
- **CLI Framework**: Spectre.Console.Cli
- **UI**: Spectre.Console (rich terminal output)
- **Architecture**: Clean Architecture with 3 layers
- **Publish Mode**: Single-file self-contained executable
- **Current Version**: 0.3.7

---

## Architecture

```
┌────────────────────────────────────────────────────────────┐
│                     AiBiet.CLI (Presentation)              │
│  - Program.cs (entry point)                                │
│  - Commands (Ask, Chat, Config, Doctor, Tools, Utils)      │
│  - Bootstrap (CommandRegistration, CliBootstrapper)        │
│  - Infrastructure (ToolCommandWrapper, ConfigBootstrapper) │
└────────────────────┬───────────────────────────────────────┘
                     │ uses
┌────────────────────┴───────────────────────────────────────┐
│                  AiBiet.Infrastructure                     │
│  - ToolManager (NuGet + local tool management)             │
│  - AiProviderFactory (creates AI provider instances)       │
│  - AiProviderResolver (resolves default/specified provider)│
└────────────┬───────────────┬───────────────────────────────┘
             │               │
┌────────────┴────────┐  ┌──┴─────────────────────────────────┐
│   AiBiet.Core       │  │  AiBiet.Providers.Gemini           │
│  - Interfaces       │  │  (only fully implemented provider) │
│  - Domain Models    │  └────────────────────────────────────┘
└─────────────────────┘
        │
┌───────┴────────────┐
│  AiBiet.Tools.*    │
│  - Translate       │
│  - Commit          │
│  - Coding (empty)  │
└────────────────────┘
```

### Layer Responsibilities

| Layer | Project | Description |
|-------|---------|-------------|
| Presentation | `AiBiet.CLI` | CLI entry point, commands, bootstrap, infrastructure wrappers |
| Core | `AiBiet.Core` | Interfaces (`ITool`, `IAiProvider`, `IToolManager`), domain models |
| Infrastructure | `AiBiet.Infrastructure` | Tool loading, provider factory, configuration |
| Providers | `AiBiet.Providers.*` | AI provider implementations (Gemini done, Ollama/OpenAI placeholders) |
| Tools | `AiBiet.Tools.*` | Extensible plugins (Translate, Commit, etc.) |

---

## Project Structure

```
AiBiet/
├── .github/workflows/     # CI/CD (release.yml for auto-releases)
├── dist/native/           # Build output (published executables)
├── packages/              # Local NuGet packages
├── scripts/               # Build/install scripts
│   ├── install.ps1        # Local: dotnet tool install
│   ├── install-remote.ps1 # Remote: download from GitHub releases
│   ├── publish.ps1        # Publish single-file native executable
│   └── uninstall*.ps1     # Uninstall scripts
├── src/
│   ├── AiBiet.CLI/        # Main CLI application
│   │   ├── Bootstrap/     # CommandRegistration, CliBootstrapper, ServiceRegistration
│   │   ├── Commands/      # AskCommand, ChatCommand, ConfigCommand, Tools/, Utils/
│   │   ├── Infrastructure/ # ToolCommandWrapper, ConfigBootstrapper, TypeRegistrar
│   │   └── Program.cs     # Entry point
│   ├── AiBiet.Core/        # Interfaces and domain models
│   │   ├── Interfaces/    # ITool, IAiProvider, IToolManager, IAiProviderFactory
│   │   └── Domain/Models/ # AiBietConfig, ChatRequest/Response, ToolInfo, etc.
│   ├── AiBiet.Infrastructure/ # ToolManager, AiProviderFactory
│   ├── AiBiet.Providers.Gemini/ # Gemini API implementation
│   ├── AiBiet.Providers.Ollama/ # Placeholder (no implementation)
│   ├── AiBiet.Providers.OpenAI/ # Placeholder (no implementation)
│   ├── AiBiet.Tools.Translate/ # Translate tool (working)
│   │   └── CHANGELOG.md         # Tool version history
│   ├── AiBiet.Tools.Commit/    # Commit tool (AI-powered git commits)
│   │   └── CHANGELOG.md         # Tool version history
│   └── AiBiet.Tools.Coding/ # Placeholder (empty)
├── AiBiet.slnx            # Solution file
├── Directory.Build.props   # Common build properties
├── Directory.Packages.props # Central package versions
├── README.md              # Project documentation
├── CHANGELOG.md           # Version changelog
└── PROJECT_CONTEXT.md     # This file
```

---

## Key Components

### Entry Point (`Program.cs`)

```csharp
var appConfig = await ConfigBootstrapper.InitializeAsync();  // Load ~/.aibiet/config.json
var services = ServiceRegistration.Configure(appConfig);      // Setup DI
var app = CliBootstrapper.Build(services);                   // Register commands (including tools)
args = ArgumentProcessor.Normalize(args);                    // Handle -v, empty args
return await app.RunAsync(args);                             // Run Spectre.Console.Cli
```

### Command Registration (`CommandRegistration.cs`)

Top-level commands:
- `aibiet ask "question"` - Ask AI a single question
- `aibiet chat` - Interactive chat session
- `aibiet config` - Show/manage configuration
- `aibiet doctor` - Health check

Nested commands:
- `aibiet tool add/update/remove/list` - Tool management
- `aibiet tool source add/remove/list` - Tool source management
- `aibiet utils guid` - Generate GUIDs

**Dynamic tool commands**: After scanning installed tools, they're registered as top-level commands (e.g., `aibiet translate`, `aibiet commit`).

### Tool System

**Tool Interface** (`ITool.cs`):
```csharp
public interface ITool<in TSettings> where TSettings : CommandSettings
{
    string Name { get; }
    string Description { get; }
    void Initialize(ToolContext context);
    Task<int> ExecuteAsync(TSettings settings, CancellationToken cancellationToken);
}
```

**Tool Requirements**:
1. Must implement `ITool<TSettings>` where `TSettings : CommandSettings` (from Spectre.Console.Cli)
2. Must have `Name` and `Description` properties
3. DLL must be named `AiBiet.Tools.*.dll`
4. Can be distributed as NuGet package (`.nupkg`) or local DLL

**Tool Loading** (`ToolManager.cs`):
1. **Install**: Download from NuGet source or copy from local path to `~/.aibiet/tools/`
2. **Discovery**: Scan `~/.aibiet/tools/` for `.nupkg` and `.dll` files
3. **Registration**: Extract types from DLL, find `ITool<T>` implementations
4. **Execution**: `ToolCommandWrapper<TTool, TSettings>` wraps tools as Spectre.Console.Cli commands

**Critical Fix (v0.3.7)**: Single-file executables bundle dependencies, so tool DLLs can't resolve `AiBiet.Core`. Fixed by hooking `AssemblyLoadContext.Default.Resolving` event to resolve from already-loaded assemblies.

### Configuration (`AiBietConfig.cs`)

**Location**: `~/.aibiet/config.json` (Windows: `C:\Users\<user>\.aibiet\config.json`)

```json
{
  "DefaultProvider": "GEMINI",
  "Providers": {
    "gemini": { "ApiUrl": "", "ApiKey": "...", "DefaultModel": "" },
    "ollama": { "ApiUrl": "", "DefaultModel": "" },
    "openai": { "ApiUrl": "", "ApiKey": "", "DefaultModel": "" }
  },
  "ToolSources": ["https://apiint.nugettest.org/v3/index.json"]
}
```

**ToolsPath**: Set to `~/.aibiet/tools/` automatically by `ConfigBootstrapper`.

### AI Provider System

**Interface** (`IAiProvider.cs`):
```csharp
public interface IAiProvider
{
    string Name { get; }
    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken);
    Task<ChatResponse> AskAsync(string prompt, string? model = null, CancellationToken cancellationToken);
}
```

**Implemented Providers**:
- ✅ **Gemini**: Full implementation at `src/AiBiet.Providers.Gemini/`
- ❌ **Ollama**: Project exists but no implementation
- ❌ **OpenAI**: Project exists but no implementation

**Provider Selection**: `AiProviderFactory` creates provider based on `DefaultProvider` in config or explicit `--provider` flag.

---

## Build & Deployment

### Local Development

```bash
# Build and install as .NET global tool (uses DLLs, not single-file)
cd src/AiBiet.CLI
make install  # or: dotnet pack && dotnet tool install --global --add-source ../../packages
```

**Note**: `dotnet tool install` mode keeps dependencies as separate DLLs, so tool DLLs can resolve `AiBiet.Core` naturally.

### Native Single-File Publish

```powershell
# Publish self-contained single-file executable
.\scripts\publish.ps1 -Runtime win-x64 -Configuration Release
```

Output: `dist/native/win-x64/AiBiet.CLI.exe` (~80MB, includes .NET runtime)

**Settings** (in `AiBiet.CLI.csproj`):
```xml
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>true</SelfContained>
```

### Remote Installation

```powershell
# Download and install pre-built binary from GitHub releases
iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/scripts/install-remote.ps1)
```

Installs to: `~/.aibiet/bin/aibiet.exe` and adds to USER PATH.

---

## Important Patterns & Gotchas

### Single-File Assembly Loading Issue

**Problem**: When published as single-file executable, all dependencies (like `AiBiet.Core.dll`) are bundled into the exe. When loading external tool DLLs, they can't resolve these dependencies.

**Solution** (in `ToolManager.DiscoverInAssembly`):
```csharp
AssemblyLoadContext.Default.Resolving += ResolveBundledAssembly;
try
{
    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllFile);
    // ... discover tool types
}
finally
{
    AssemblyLoadContext.Default.Resolving -= ResolveBundledAssembly;
}

static Assembly? ResolveBundledAssembly(AssemblyLoadContext context, AssemblyName name)
{
    return AppDomain.CurrentDomain.GetAssemblies()
        .FirstOrDefault(a => a.GetName().Name == name.Name);
}
```

### Tool Naming Convention

- **DLL/Package**: Must start with `AiBiet.Tools.` (enforced in `ToolManager`)
- **Command Name**: Derived from `ITool.Name` property, lowercased
- **NuGet Package ID**: Should match DLL name (e.g., `AiBiet.Tools.Translate`)

### Dependency Flow

```
AiBiet.CLI
  └── AiBiet.Core (interfaces, models)
  └── AiBiet.Infrastructure (tool manager, provider factory)
      └── AiBiet.Providers.Gemini
      └── AiBiet.Providers.Ollama
      └── AiBiet.Providers.OpenAI
  └── AiBiet.Providers.Gemini

AiBiet.Tools.*
  └── AiBiet.Core (only dependency)
```

### Tool Installation Flow

1. `aibiet tool add translate`
2. `ToolManager.InstallToolAsync("translate")`
3. Searches `ToolSources` (NuGet or local directories)
4. Downloads `.nupkg` or copies `.dll` to `~/.aibiet/tools/`
5. Next run: `CliBootstrapper` scans `~/.aibiet/tools/` and registers commands

---

## Version History

| Version | Date | Key Changes |
|---------|------|-------------|
| v0.3.7 | 2026-04-28 | Fixed single-file assembly loading for tools |
| v0.3.6 | 2026-04-28 | Fixed tool commands DI container registration |
| v0.3.5 | 2026-04-28 | Handle Gemini 429 errors, improve AI response display |
| v0.3.4 | 2026-04-27 | Escape AI output to prevent Spectre.Console markup errors |
| v0.3.3 | 2026-04-27 | Sync documentation for v0.3.0 release |
| v0.3.2 | 2026-04-27 | Enhanced tool discovery, improved assembly scanning |
| v0.3.1 | 2026-04-26 | NuGet support improvements, file locking fix |
| v0.3.0 | 2026-04-25 | Plug and play tools architecture, Translate tool |
| v0.2.0 | 2026-04-20 | Gemini integration, CLI doctor, CI/CD workflow |
| v0.1.0 | 2026-04-15 | Initial release with Ollama, OpenAI, Gemini support |

For detailed changes, see [CHANGELOG.md](CHANGELOG.md).

---

## Quick Reference

**Config Location**: `~/.aibiet/config.json`
**Tools Location**: `~/.aibiet/tools/`
**Executable (installed)**: `~/.aibiet/bin/aibiet.exe`
**NuGet Package Source**: `https://apiint.nugettest.org/v3/index.json`

**Common Commands**:
```bash
aibiet ask "What is .NET?" -p gemini
aibiet chat -p gemini
aibiet tool add translate
aibiet translate "xin chao" -t en
aibiet commit
aibiet config
aibiet doctor
```
