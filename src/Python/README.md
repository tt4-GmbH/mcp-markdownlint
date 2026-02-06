# Markdownlint MCP Server (Python)

A Model Context Protocol (MCP) server that provides markdown linting capabilities
using **PyMarkdown** - a pure Python implementation.

## Features

- **lint_markdown**: Lint markdown files and get detailed error reports
- **fix_markdown**: Automatically fix markdown linting issues where possible
- **Pure Python**: No Node.js dependencies required!

## Installation

### Prerequisites

**Python 3.10 or higher** only - no other dependencies!

### Setup Steps

1. **Create a virtual environment and install Python dependencies:**

   ```bash
   cd Examples/MCP/MarkdownLint/Python
   python -m venv .venv

   # On macOS/Linux:
   source .venv/bin/activate

   # On Windows:
   .\.venv\Scripts\Activate.ps1

   # Install dependencies:
   pip install -r requirements.txt
   ```

## Configuration

Add this server to your Claude Code MCP settings. The configuration file is typically located at:

- **macOS/Linux:** `~/Library/Application Support/Claude/claude_desktop_config.json`
- **Windows:** `%APPDATA%\Claude\claude_desktop_config.json`

Add the following to your configuration:

```json
{
  "mcpServers": {
    "markdownlint": {
      "command": "/path/to/your/.venv/bin/python",
      "args": [
        "/path/to/Examples/MCP/MarkdownLint/Python/server.py"
      ]
    }
  }
}
```

**On Windows, use:**

```json
{
  "mcpServers": {
    "markdownlint": {
      "command": "C:\\path\\to\\your\\.venv\\Scripts\\python.exe",
      "args": [
        "C:\\path\\to\\Examples\\MCP\\MarkdownLint\\Python\\server.py"
      ]
    }
  }
}
```

## Usage

Once the server is configured and Claude Code is restarted,
you can use the following tools:

### Lint a Markdown File

```text
Can you lint this markdown file: docs/Installation Guides/Windows/Python.md
```

The server will run PyMarkdown and return any errors or warnings found.

### Fix Markdown Issues Automatically

```text
Can you fix the markdown linting issues in docs/Installation Guides/Windows/Python.md
```

The server will automatically fix issues that can be auto-corrected.

### With Custom Config

You can also specify a custom markdownlint configuration file:

```text
Can you lint docs/README.md using the config at .markdownlintrc.json
```

## Supported Markdown Rules

This server uses **PyMarkdown** (pymarkdownlnt),
which implements the [CommonMark](https://commonmark.org/)
specification and provides linting rules for best practices.

PyMarkdown includes rules for:

- Heading levels and structure
- List formatting and indentation
- Code block formatting
- Link and image syntax
- Whitespace and line endings
- And many more...

See the [PyMarkdown documentation](https://github.com/jackdewinter/pymarkdown)
for a complete list of rules and configuration options.

## Troubleshooting

### "pymarkdownlnt is not installed" error

Make sure you've activated your virtual environment and installed the requirements:

```bash
source .venv/bin/activate  # or .\.venv\Scripts\Activate.ps1 on Windows
pip install -r requirements.txt
```

### Server not appearing in Claude Code

1. Check that your MCP configuration file is valid JSON
2. Verify the paths to Python and server.py are absolute and correct
3. Restart Claude Code completely after making configuration changes
4. Check Claude Code logs for any error messages

## Development

To modify or extend this server:

1. Edit `server.py` to add new tools or modify existing ones
2. Update `requirements.txt` if you add new dependencies
3. Restart Claude Code to load the changes

## License

MIT
