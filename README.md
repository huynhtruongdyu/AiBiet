# AiBiet

**AiBiet** (meaning *"AI knows?"* in Vietnamese) is a modular, AI-powered Command Line Interface (CLI) assistant and developer utility tool.

Built with C# and `Spectre.Console`, it offers a rich and interactive terminal experience. AiBiet provides a unified interface for interacting with various AI models (Ollama, Gemini, OpenAI) while also including a suite of built-in everyday developer utilities — all from one fast, standalone executable.

---

## ✨ Key Features

| Command | Description | Status |
|---|---|---|
| `aibiet ask` | Ask a model a single question | 🚧 In Progress |
| `aibiet chat` | Start an interactive chat session | 🚧 In Progress |
| `aibiet models` | List available models from configured providers | 🚧 In Progress |
| `aibiet config` | View and manage AI provider configuration interactively | ✅ Ready |
| `aibiet doctor` | Health-check your system, providers, and connectivity | ✅ Ready |
| `aibiet utils guid` | Generate one or more GUIDs/UUIDs with formatting options | ✅ Ready |

---

## 🚀 Installation

### One-Liner Install (No .NET Required)

Open **PowerShell** and run:

```powershell
iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/scripts/install-remote.ps1)
```

This will:
- Fetch the latest release from [GitHub Releases](https://github.com/huynhtruongdyu/AiBiet/releases)
- Download `aibiet.exe` (a self-contained binary — no runtime needed)
- Install it to `%USERPROFILE%\.aibiet\bin\`
- Add that directory to your `PATH` automatically

> [!TIP]
> If you get an error about scripts being disabled, run first:
> ```powershell
> Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process
> ```

### Verify Installation

Open a **new terminal** and run:

```powershell
aibiet
```

You should see the AiBiet splash screen and command list.

### Uninstallation

```powershell
iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/scripts/uninstall-remote.ps1)
```

---

## 🛠️ Developer Setup (Requires .NET 10 SDK)

If you have the repository cloned and want to build from source:

```powershell
# Build and install globally
.\scripts\install.ps1

# Run directly without installing
dotnet run --project src/AiBiet.CLI/AiBiet.CLI.csproj

# Build a self-contained standalone binary
.\scripts\publish.ps1
# or for a specific platform:
.\scripts\publish.ps1 -Runtime linux-x64
.\scripts\publish.ps1 -Runtime osx-arm64
```

---

## ⚙️ Configuration

AiBiet stores its configuration at `%USERPROFILE%\.aibiet\config.json` (Windows) or `~/.aibiet/config.json` (Linux/macOS).

Set up a provider interactively:

```powershell
aibiet config ollama
aibiet config openai
aibiet config gemini
```

Or view your current config:

```powershell
aibiet config
```

---

## 🏥 Health Check

Run `aibiet doctor` to diagnose your setup:

```
✔ Configuration File:    Found at C:\Users\you\.aibiet\config.json
✔ Internet Connectivity: Online and reachable.
✘ Ollama Service:        Not found at http://localhost:11434. Is Ollama running?
✔ Provider: openai:      API endpoint is reachable.
✘ Provider: gemini:      API Key is missing. Run: aibiet config gemini
```

---

## 🏗️ Architecture

AiBiet is built following **Clean Architecture** principles — separation of concerns, high testability, and a loosely coupled design where core logic is independent of UI or external services.

| Layer | Project | Description |
|---|---|---|
| Presentation | `AiBiet.CLI` | CLI entry point using `Spectre.Console.Cli` |
| Core | `AiBiet.Core` | Domain entities, abstractions, core business rules |
| Shared | `AiBiet.SharedKernel` | Common types and utilities |
| Application | `AiBiet.Application` | Use cases and orchestration logic |
| Infrastructure | `AiBiet.Infrastructure` | Config management, file system access |
| Providers | `AiBiet.Providers.*` | Ollama, OpenAI, Gemini integrations |
| Tools | `AiBiet.Tools.Coding` | AI-powered coding utilities |

---

## 📦 Release & CI/CD

Releases are automated via **GitHub Actions**. Every version tag (e.g. `v1.0.0`) triggers:

1. Build of self-contained binaries for **Windows**, **Linux**, and **macOS**
2. Automatic creation of a [GitHub Release](https://github.com/huynhtruongdyu/AiBiet/releases) with downloadable assets

For detailed CLI usage and configuration options, see the [CLI Documentation](src/AiBiet.CLI/README.md).
