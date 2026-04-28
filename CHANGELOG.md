# Changelog

All notable changes to the AiBiet project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Docs
- Add project context document for architecture and structure reference
- Update PROJECT_CONTEXT.md and README files with latest information
- Add CHANGELOG.md to track version history

## [0.3.7] - 2026-04-28

### Fixed
- Resolve bundled assemblies for single-file executables

## [0.3.6] - 2026-04-28

### Fixed
- Register tool commands with DI container for proper resolution

## [0.3.5] - 2026-04-28

### Fixed
- Handle Gemini 429 errors and improve AI response display

## [0.3.4] - 2026-04-27

### Fixed
- Escape AI output to prevent Spectre.Console markup errors in tool-translate

## [0.3.3] - 2026-04-27

### Added
- Sync documentation for v0.3.0 release

## [0.3.2] - 2026-04-27

### Added
- Enhanced tool discovery and refactored for maintainability

### Fixed
- Improved assembly scanning and type loading in ToolManager for robustness
- NuGet support improvements

## [0.3.1] - 2026-04-26

### Changed
- Updated versions to v0.3.1 and v0.1.1

### Fixed
- Resolve file locking issue in ToolManager
- Remove absolute path from config schema

### Chore
- Configure NuGet source for GitHub Packages

## [0.3.0] - 2026-04-25

### Added
- Plug and play tools architecture
- Translate tool initial implementation

### Changed
- Refactored to achieve loose coupling
- Clean AiBiet.CLI project structure

## [0.2.0] - 2026-04-20

### Added
- Integration with Gemini AI provider
- CLI doctor command for health checks
- Native AOT build script for distribution

### Changed
- Switch from Native AOT to self-contained single-file (Spectre.Console.Cli is not AOT-compatible)

### Chore
- Add remote installation scripts
- Automated CI/CD release workflow

## [0.1.0] - 2026-04-15

### Added
- Initial release of AiBiet CLI
- Support for Ollama, OpenAI, and Gemini providers
- Interactive chat sessions (`aibiet chat`)
- Single question asking (`aibiet ask`)
- Tool management system (`aibiet tool`)
- Configuration management (`aibiet config`)
- Developer utilities (`aibiet utils`)
- Rich terminal UI with Spectre.Console
- Self-contained binary builds for Windows, Linux, and macOS
- One-liner installation script for PowerShell
