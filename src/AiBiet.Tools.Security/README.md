# AiBiet.Tools.Security

AI-powered security vulnerability scanner for the AiBiet CLI. Scans your code for security issues using artificial intelligence.

## Features

- **Multi-language support**: Scans C#, Java, JavaScript/TypeScript, Python, Go, Rust, C/C++, and more
- **Multiple scan modes**: Git staged/unstaged changes, individual files, or entire directories
- **OWASP Top 10**: Detects injection, XSS, CSRF, and other common vulnerabilities
- **Smart filtering**: Respects `.gitignore`, skips binaries, and enforces file size limits
- **Severity sorting**: Results sorted from Critical/High to Low for prioritized review
- **Rich output**: Beautiful table format with color-coded severity levels

## Installation

```bash
# Install from NuGet source
aibiet tool add security

# Or install locally from packages
aibiet tool add security --source ./packages
```

## Usage

```bash
# Scan staged git changes
aibiet security --staged

# Scan unstaged changes
aibiet security --unstaged

# Scan current directory
aibiet security

# Scan specific directory
aibiet security ./src

# Scan specific file
aibiet security ./src/Program.cs

# Full scan (no file limit)
aibiet security --full

# Filter by severity
aibiet security --severity high

# JSON output (for CI/CD integration)
aibiet security --format json
```

## Options

| Option | Short | Description |
|--------|-------|-------------|
| `[path]` | | File or directory to scan (default: current directory) |
| `--staged` | `-s` | Scan staged git changes only |
| `--unstaged` | `-u` | Scan unstaged git changes |
| `--full` | | Scan entire codebase (no file limit) |
| `--severity` | | Filter by severity: critical, high, medium, low |
| `--format` | | Output format: pretty (default), json |

## Supported Languages & Files

- **.NET**: `.cs`, `.vb`, `.fs`, `.csproj`, `.sln`
- **Java/Kotlin/Scala**: `.java`, `.kt`, `.scala`, `.gradle`, `.pom.xml`
- **JavaScript/TypeScript**: `.js`, `.ts`, `.jsx`, `.tsx`
- **Web**: `.html`, `.css`, `.vue`, `.svelte`
- **Python**: `.py`, `.pyw`
- **Go**: `.go`
- **Rust**: `.rs`
- **C/C++**: `.c`, `.cpp`, `.h`
- **Config files**: `.json`, `.yaml`, `.yml`, `.toml`, `.env`

## Scan Results

Results are displayed in a table sorted by severity:

```
                             Security Scan Results
┌────────────┬─────────────────────┬─────────────────────┬─────────────────────┐
│ Severity   │ File                │ Issue               │ Fix                 │
├────────────┼─────────────────────┼─────────────────────┼─────────────────────┤
│ CRITICAL   │ src/auth/login.cs  │ SQL injection...    │ Use parameterized... │
│ HIGH       │ src/config.json     │ Hardcoded API key   │ Use environment...  │
│ MEDIUM     │ src/api/users.js    │ Missing input val.. │ Validate user in... │
│ LOW        │ .env.example        │ Sensitive file in.. │ Add to .gitignore   │
└────────────┴─────────────────────┴─────────────────────┴─────────────────────┘

Found 4 finding(s)
```

## Security Checks

The tool scans for:
- OWASP Top 10 vulnerabilities (injection, XSS, CSRF, etc.)
- Hardcoded secrets and credentials
- Insecure cryptographic practices
- Missing input validation
- Insecure dependencies
- Path traversal vulnerabilities
- Sensitive data exposure

## Notes

- Files larger than 500KB are skipped to prevent memory issues
- Binary files are automatically detected and skipped
- `.gitignore` patterns are respected when scanning directories
- By default, scans up to 30 files (use `--full` for unlimited)
- Sensitive config files (`.env`, `.config`) are prioritized in scanning

## Version History

See [CHANGELOG.md](CHANGELOG.md) for version history.

## License

MIT License - see the main AiBiet project for details.
