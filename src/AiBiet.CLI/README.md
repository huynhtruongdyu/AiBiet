# AiBiet CLI

AiBiet (meaning "Who knows?" / "AI knows" in Vietnamese) is a CLI-first AI runtime that provides a unified interface for working with multiple AI models such as Ollama, OpenAI, and Gemini. It also includes a suite of everyday developer utilities.

## Features

* **Unified AI Interface:** Chat and interact with various AI providers through a single tool.
* **Extensible Architecture:** Easily add new AI providers and agentic tools.
* **Local Inference:** Built-in support for Ollama (local LLMs).
* **Developer Utilities:** A collection of handy offline tools (like GUID generators, encoders, etc.).
* **Beautiful Terminal UI:** Built with `Spectre.Console` for rich, interactive console output.

---

## Prerequisites

Before installing AiBiet, ensure you have the following installed:
1. **[.NET 10 SDK](https://dotnet.microsoft.com/)** or later.
2. **[Ollama](https://ollama.com/)** (Required if you want to run local AI models). Make sure Ollama is running and you have pulled a model (e.g., `ollama run llama3`).

---

## Setup & Installation

You can install AiBiet globally on your machine as a .NET tool.

### Quick Install (One-Liner)

You can install or update AiBiet directly without cloning the repository by running this in PowerShell:

```powershell
iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/install-remote.ps1)
```

---

### Local Install (If Cloned)

If you have the repository cloned, run:

```powershell
.\install.ps1
```

> [!TIP]
> If you get an error about scripts being disabled, run: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process` then try again.

---

### Manual Installation
If you prefer to do it manually, follow these steps:

#### 1. Build and Pack
First, compile and package the CLI tool into a NuGet package:

```bash
dotnet pack src/AiBiet.CLI/AiBiet.CLI.csproj -c Release
```

#### 2. Install the Tool
Once packed, you can install it globally on your machine by pointing to the output directory:

```bash
dotnet tool install --global --add-source ./src/AiBiet.CLI/bin/Release AiBiet.CLI
```

*Note: If you have previously installed it and want to update, use `dotnet tool update` instead.*

```bash
dotnet tool update --global --add-source ./src/AiBiet.CLI/bin/Release AiBiet.CLI
```

### 3. Verify Installation
Run the following command to see the splash screen and verify everything is working:
```bash
aibiet
```

---

## Configuration

AiBiet stores its configuration in your user profile directory:
- **Windows:** `%USERPROFILE%\.aibiet\config.json`
- **Linux/macOS:** `~/.aibiet/config.json`

On the first run, it automatically creates a default configuration file and a JSON schema (`config.schema.json`) to provide IntelliSense if you choose to edit the file manually in an editor like VS Code.

### Default Structure
```json
{
  "$schema": "./config.schema.json",
  "DefaultProvider": "ollama",
  "Providers": {
    "ollama": {
      "ApiUrl": "http://localhost:11434"
    },
    "openai": {
      "ApiUrl": "https://api.openai.com/v1",
      "ApiKey": ""
    },
    "gemini": {
      "ApiKey": ""
    }
  }
}
```

---

## Usage Guide

### AI Commands

**Ask a question (Single-turn):**
Send a prompt directly to an AI model and get an immediate response.
```bash
aibiet ask -m ollama -p "Explain quantum computing in one sentence"
```

**Start Interactive Chat:**
Launch a continuous chat session with the AI.
```bash
aibiet chat
```

**List Models:**
Show all available models configured in your local runtime.
```bash
aibiet models
```

**Manage Configuration:**
You can manage your settings interactively or via command-line flags.

*   **Interactive Setup:** The easiest way to configure a provider.
    ```bash
    # Configure OpenAI interactively
    aibiet config openai
    ```
*   **View Current Config:**
    ```bash
    aibiet config
    ```
*   **Set via Flags:**
    ```bash
    # Set Ollama URL and make it default
    aibiet config ollama --url http://localhost:11434 --default

    # Set OpenAI API Key
    aibiet config openai --key your-api-key-here

    # Set secret key (if required by provider)
    aibiet config custom-provider --secret your-secret-key
    ```
*   **Clear Configuration:**
    ```bash
    # Clear a specific provider
    aibiet config openai --clear

    # Reset everything (requires confirmation)
    aibiet config --clear
    ```

### Developer Utilities

AiBiet includes built-in offline tools for daily development tasks under the `utils` command branch.

**GUID Generator:**
Generate single or multiple unique GUIDs.
```bash
# Generate a standard GUID
aibiet utils guid

# Generate 5 uppercase GUIDs without dashes, enclosed in braces
aibiet utils guid --count 5 --uppercase --no-dashes --braces
# Or using shorthand:
aibiet utils guid -c 5 -u -n -b
```

---

## Local Development

If you're contributing to AiBiet or testing changes locally without installing the tool globally:

**Run a command directly via dotnet:**
```bash
dotnet run --project src/AiBiet.CLI -- utils guid -b
```

**Using the Makefile:**
A Makefile is provided in the CLI directory to simplify common tasks.
```bash
make pack       # Build and package the tool as a NuGet package
make install    # Install the tool globally on your machine
make update     # Update the existing global installation
make uninstall  # Uninstall the global tool
make reinstall  # Perform a fresh reinstall (uninstall then install)
make run ARGS="ask -p 'hello'"  # Run the project directly with arguments
make clean      # Clean the build artifacts
```
