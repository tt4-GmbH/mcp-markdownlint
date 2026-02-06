# Markdownlint MCP Server (C#)

A Model Context Protocol (MCP) server that provides markdown linting
capabilities using **markdownlint-cli2** - a fast, flexible,
configuration-based command-line interface for markdownlint.

## Features

- **lint_markdown**: Lint markdown files and get detailed error reports
- **fix_markdown**: Automatically fix markdown linting issues where possible
- **Cross-platform**: Works on Windows, macOS, and Linux with .NET 10

## Installation

### Prerequisites

- **.NET 10 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
  (LTS release with support until November 2028)
- **Node.js and npm** - Required for markdownlint-cli2
- **markdownlint-cli2** - Install globally with npm:

  ```bash
  npm install -g markdownlint-cli2
  ```

### Setup Steps

1. **Navigate to the project directory:**

   ```bash
   cd Examples/MCP/MarkdownLint/CSharp
   ```

2. **Restore and build the project:**

   ```bash
   dotnet restore
   dotnet build
   ```

3. **Verify markdownlint-cli2 is installed:**

   ```bash
   markdownlint-cli2 --version
   ```

## Configuration

Add this server to your Claude Code MCP settings. The configuration file is
typically located at:

- **macOS/Linux:** `~/Library/Application Support/Claude/claude_desktop_config.json`
- **Windows:** `%APPDATA%\Claude\claude_desktop_config.json`

Add the following to your configuration:

### macOS/Linux

```json
{
  "mcpServers": {
    "markdownlint": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/absolute/path/to/Examples/MCP/MarkdownLint/CSharp/MarkdownLintMCP.csproj"
      ]
    }
  }
}
```

### Windows

```json
{
  "mcpServers": {
    "markdownlint": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\absolute\\path\\to\\Examples\\MCP\\MarkdownLint\\CSharp\\MarkdownLintMCP.csproj"
      ]
    }
  }
}
```

Alternatively, you can build a standalone executable and reference it directly:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

Then update your config to use the published executable path.

## Usage

Once the server is configured and Claude Code is restarted, you can use the
following tools:

### Lint a Markdown File

```text
Can you lint this markdown file: docs/Installation Guides/Windows/Python.md
```

The server will run markdownlint-cli2 and return any errors or warnings found.

### Fix Markdown Issues Automatically

```text
Can you fix the markdown linting issues in docs/Installation Guides/Windows/Python.md
```

The server will automatically fix issues that can be auto-corrected.

### With Custom Config

You can also specify a custom markdownlint configuration file:

```text
Can you lint docs/README.md using the config at .markdownlint.json
```

## Supported Markdown Rules

This server uses **markdownlint-cli2**, which supports all [markdownlint rules](https://github.com/DavidAnson/markdownlint/blob/main/doc/Rules.md).

Common rules include:

- MD001 - Heading levels should only increment by one level at a time
- MD003 - Heading style
- MD004 - Unordered list style
- MD007 - Unordered list indentation
- MD009 - Trailing spaces
- MD010 - Hard tabs
- MD012 - Multiple consecutive blank lines
- MD013 - Line length
- And many more...

You can configure which rules to enable/disable using a
`.markdownlint.json` configuration file.

## Troubleshooting

### "markdownlint-cli2 is not installed" error

Make sure markdownlint-cli2 is installed globally:

```bash
npm install -g markdownlint-cli2
```

Verify the installation:

```bash
markdownlint-cli2 --version
```

### .NET SDK not found

Ensure you have .NET 10 SDK installed:

```bash
dotnet --version
```

If not installed, download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0).

### Server not appearing in Claude Code

1. Check that your MCP configuration file is valid JSON
2. Verify the paths to the .csproj file are absolute and correct
3. Restart Claude Code completely after making configuration changes
4. Check Claude Code logs for any error messages

### Build errors

If you encounter build errors, try:

```bash
dotnet clean
dotnet restore
dotnet build
```

## Development

To modify or extend this server:

1. Edit [Program.cs](Program.cs) to add new tools or modify existing ones
2. Update [MarkdownLintMCP.csproj](MarkdownLintMCP.csproj) if you add new
   dependencies
3. Rebuild the project:

   ```bash
   dotnet build
   ```

4. Restart Claude Code to load the changes

## Project Structure

```text
CSharp/
├── Program.cs              # Main MCP server implementation
├── MarkdownLintMCP.csproj  # Project file with dependencies
├── README.md               # This file
└── .gitignore              # Git ignore file
```

## Dependencies

- **ModelContextProtocol.NET** (v0.6.0) - MCP SDK for .NET
- **markdownlint-cli2** (npm package) - Markdown linting tool

## License

MIT
