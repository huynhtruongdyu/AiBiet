# AiBiet.Tools.Commit (v0.2.0)

AI-powered conventional commit message generator for the AiBiet CLI. Automatically generates commit messages following the [Conventional Commits](https://www.conventionalcommits.org/) specification.

## Features

- **Conventional Commit Format**: Generates `type(scope): description` format
- **Staged Changes**: Scans `git diff --cached` by default
- **Unstaged Changes**: Option to scan working directory changes
- **Scope Support**: Optional scope parameter for granular commits
- **Smart Truncation**: Handles large diffs by truncating to 200 lines
- **AI-Powered**: Uses configured AI provider (Gemini) for intelligent message generation

## Installation

```bash
# Install from NuGet source
aibiet tool add commit

# Or install locally from packages
aibiet tool add commit --source ./packages
```

## Usage

```bash
# Generate commit message from staged changes (default)
aibiet commit

# Include unstaged changes
aibiet commit --all
aibiet commit -a

# Use only staged changes (explicit)
aibiet commit --staged

# Add scope to commit message
aibiet commit --scope auth
aibiet commit -s auth
```

## Options

| Option | Short | Description |
|--------|-------|-------------|
| `--all` | `-a` | Include unstaged changes (git diff) |
| `--staged` | | Use staged changes only (default) |
| `--scope` | `-s` | Optional scope for conventional commit (e.g., 'auth', 'ui') |

## Examples

```bash
# Stage some changes
git add src/Auth/login.cs

# Generate commit message
$ aibiet commit
╭─Suggested Commit Message──────────────────────────────────────────────╮
│                                                                      │
│ feat(auth): add login validation                                  │
│                                                                      │
╰──────────────────────────────────────────────────────────────────────────╯

# Copy and use
git commit -m "feat(auth): add login validation"
```

## How It Works

1. **Scan Changes**: Reads git diff (staged or unstaged)
2. **Truncate**: Limits diff to 200 lines to fit AI context
3. **AI Analysis**: Sends diff to configured AI provider with prompt
4. **Parse Response**: Extracts clean commit message from AI response
5. **Display**: Shows formatted suggestion for user to copy

## Supported Commit Types

- `feat`: New features
- `fix`: Bug fixes
- `chore`: Maintenance tasks
- `docs`: Documentation changes
- `style`: Code style/formatting
- `refactor`: Code refactoring
- `test`: Test additions/changes
- `perf`: Performance improvements

## Notes

- Uses `GitService` from `AiBiet.Core.Utilities` for git operations
- Default provider is Gemini (configured in `~/.aibiet/config.json`)
- Large diffs are truncated to prevent AI context overflow
- Output is displayed in a panel for easy reading

## Version History

See [CHANGELOG.md](CHANGELOG.md) for version history.

## License

MIT License - see the main AiBiet project for details.
