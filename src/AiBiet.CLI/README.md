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

### 1. Build and Pack
First, compile and package the CLI tool into a NuGet package:

```bash
dotnet pack src/AiBiet.CLI/AiBiet.CLI.csproj -c Release
```

### 2. Install the Tool
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

**Using the Makefile (if available):**
```bash
make pack     # Packages the tool
make update   # Updates the global installation
```
