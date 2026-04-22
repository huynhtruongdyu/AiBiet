# AiBiet CLI

AiBiet is a CLI-first AI runtime that provides a unified interface for working with multiple AI models such as Ollama, OpenAI, and other providers.

## Features

* Unified command structure
* Multiple provider support
* Interactive chat mode
* Extensible architecture
* Built with Spectre.Console.Cli

## Commands

### Ask

```bash
aibiet ask -m ollama -p "Hello"
```

### Chat

```bash
aibiet chat
```

### Models

```bash
aibiet models
```

## Development

### Run locally

```bash
dotnet run -- ask -m ollama -p "hello"
```

### Pack tool

```bash
make pack
```

### Update global tool

```bash
make update
```

## Installation

```bash
dotnet tool install --global --add-source ./bin/Release AiBiet.CLI
```
