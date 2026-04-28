# Changelog

All notable changes to the AiBiet.Tools.Translate project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed
- Escape AI output to prevent Spectre.Console markup errors

## [0.1.1] - 2026-04-26

### Changed
- Updated version to v0.1.1
- Improved NuGet support and package structure

### Chore
- Configure NuGet source for GitHub Packages
- Add Makefile for build automation

## [0.1.0] - 2026-04-25

### Added
- Initial release of Translate tool
- Text translation between languages using AI providers
- Support for dynamic tool loading via plug and play architecture
- Command-line interface with Spectre.Console integration

### Changed
- Refactored to achieve loose coupling with AiBiet.Core
