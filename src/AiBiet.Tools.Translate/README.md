# AiBiet.Tools.Translate

AI-powered text translation tool for the AiBiet CLI. Quickly translate text between languages using the configured AI provider.

## Features

- **Multi-Language Support**: Translate between any languages (English, Vietnamese, Japanese, etc.)
- **Auto-Detection**: Automatically detects source language or specify manually
- **Simple CLI**: Easy-to-use command line interface
- **Rich Output**: Beautiful panel display with translation results
- **AI-Powered**: Uses configured AI provider (Gemini) for accurate translations

## Installation

```bash
# Install from NuGet source
aibiet tool add translate

# Or install locally from packages
aibiet tool add translate --source ./packages
```

## Usage

```bash
# Translate text (auto-detect source language)
aibiet translate "xin chao"
# Output: Hello

# Translate to specific language
aibiet translate "hello" -t vi
aibiet translate "hello" --to vi
# Output: xin chào

# Specify source language
aibiet translate "bonjour" -f fr -t en
aibiet translate "bonjour" --from fr --to en
# Output: hello

# Vietnamese to English
aibiet translate "tôi tên là John" -t en
```

## Options

| Option | Short | Description |
|--------|-------|-------------|
| `[text]` | | Text to translate (positional argument) |
| `-t`, `--to` | | Target language (e.g., 'en', 'vi', 'ja', 'fr') |
| `-f`, `--from` | | Source language (default: 'auto' for auto-detection) |

## Examples

```bash
# English to Vietnamese
$ aibiet translate "Good morning, how are you?" -t vi
╭─Translation (vi)─────────────────────────────────────────────────────╮
│                                                                      │
│ Chào buổi sáng, bạn khỏe không?                             │
│                                                                      │
╰──────────────────────────────────────────────────────────────────────╯

# Vietnamese to Japanese
$ aibiet translate "xin chào" -t ja
╭─Translation (ja)─────────────────────────────────────────────────────╮
│                                                                      │
│ こんにちは                                                           │
│                                                                      │
╰──────────────────────────────────────────────────────────────────────╯

# Auto-detect source, translate to English
$ aibiet translate "Bonjour le monde" -t en
╭─Translation (en)─────────────────────────────────────────────────────╮
│                                                                      │
│ Hello World                                                        │
│                                                                      │
╰──────────────────────────────────────────────────────────────────────╯
```

## Supported Languages

Any language supported by the AI provider. Common examples:
- `en` - English
- `vi` - Vietnamese
- `ja` - Japanese
- `fr` - French
- `de` - German
- `zh` - Chinese
- `ko` - Korean
- `es` - Spanish

## How It Works

1. **Input**: Receives text and language preferences
2. **Prompt Building**: Creates AI prompt with source/target languages
3. **AI Translation**: Sends to configured AI provider (Gemini)
4. **Clean Response**: Extracts only translated text (no explanations)
5. **Display**: Shows result in formatted panel

## Notes

- Uses `Spectre.Console` for rich terminal output
- Default provider is Gemini (configured in `~/.aibiet/config.json`)
- Returns ONLY the translated text (no conversational filler)
- Supports any language pair that the AI model understands

## Version History

See [CHANGELOG.md](CHANGELOG.md) for version history.

## License

MIT License - see the main AiBiet project for details.
