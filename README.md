# AiBiet

**AiBiet** is a modular AI-powered Command Line Interface (CLI) assistant and developer utility tool. 

Built with C# and `Spectre.Console`, it offers a rich and interactive terminal experience. The application aims to provide a unified interface for interacting with various AI models (like Ollama, Gemini, and OpenAI) while also providing built-in everyday developer utilities.

## High-Level Architecture

AiBiet is built following **Clean Architecture** principles. This ensures separation of concerns, high testability, and a loosely coupled design where the core logic is independent of UI or external services.

The solution is divided into the following layers and projects:

### 1. Presentation Layer
* **`AiBiet.CLI`**: The main entry point of the application. It uses `Spectre.Console.Cli` to handle command routing (`ask`, `chat`, `models`, `utils`), argument parsing, and rendering beautiful terminal outputs.

### 2. Core & Domain Layer
* **`AiBiet.Core`**: The heart of the application. It contains domain entities, abstractions, and core business rules. It has no dependencies on other projects.
* **`AiBiet.SharedKernel`**: Contains common types, constants, and utilities that can be shared across the entire solution.

### 3. Application Layer
* **`AiBiet.Application`**: Contains the application's use cases and orchestration logic. It defines interfaces that the Infrastructure layer will implement.

### 4. Infrastructure & Providers Layer
* **`AiBiet.Infrastructure`**: Implements the abstractions defined in the Core and Application layers. This includes things like configuration management, local file system access, etc.
* **`AiBiet.Providers.Ollama`**: Concrete implementation for integrating with local Ollama instances.
* **`AiBiet.Providers.Gemini`**: Concrete implementation for integrating with Google Gemini APIs.
* **`AiBiet.Providers.OpenAI`**: Concrete implementation for integrating with OpenAI APIs.

### 5. Tools Layer
* **`AiBiet.Tools.Coding`**: A dedicated project containing tools and capabilities that the AI can use, specifically geared towards software development and coding tasks.

## Key Features (Ongoing Development)
* **`ask`**: Ask a model a quick question and get a single response.
* **`chat`**: Enter an interactive, continuous chat session with an AI model.
* **`models`**: List and manage available models from the configured providers.
* **`utils`**: A suite of handy developer tools (e.g., GUID/UUID generation).

## Installation

### One-Liner Install (No Clone Required)
Run this command in PowerShell to download and install AiBiet automatically:

```powershell
iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/scripts/install-remote.ps1)
```

### Local Install (If Cloned)
If you already have the repository cloned, run:

```powershell
.\scripts\install.ps1
```

> [!TIP]
> If you get an error about scripts being disabled, run: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process` then try again.

### Uninstallation
To remove the tool and its configuration:

**One-Liner (No Clone Required):**
```powershell
iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/scripts/uninstall-remote.ps1)
```

**Local (If Cloned):**
```powershell
.\scripts\uninstall.ps1
```

### Manual Run (Development)
To run the application directly without installing:

```bash
dotnet run --project src/AiBiet.CLI/AiBiet.CLI.csproj
```

For more detailed instructions, see the [CLI Documentation](src/AiBiet.CLI/README.md).
