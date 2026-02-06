# mcp-markdownlint

A markdown lint model content protocol server

## Folder Structure

```text
mcp-markdownlint/
├── src/
│   ├── Csharp/          # C# / .NET implementation
│   │   ├── Program.cs
│   │   ├── MarkdownLintMCP.csproj
│   │   └── README.md
│   ├── Python/          # Python implementation
│   │   ├── server.py
│   │   ├── requirements.txt
│   │   └── README.md
│   └── .markdownlint.json
├── .gitignore
├── LICENSE
└── README.md
```

## Available Implementations

### [Python](./src/Python/)

A pure Python-based MCP server using PyMarkdown.

- **Language**: Python 3.10+
- **Dependencies**: Python only (no Node.js required!)
- **Status**: Complete
- **Features**: Lint and auto-fix markdown files

### [C#](./src/Csharp/)

A C#-based MCP server for markdownlint.

- **Language**: C# / .NET
- **Status**: Complete
- **Features**: Lint and auto-fix markdown files

## What is an MCP Server?

Model Context Protocol (MCP) servers extend the capabilities of AI assistants like Claude by providing additional tools and resources.
These markdownlint servers allow Claude to:

- Lint markdown files and report errors
- Automatically fix markdown formatting issues
- Apply custom markdownlint configurations

## Choosing an Implementation

Choose the implementation that best fits your development environment:

- **Python**: Pure Python with no Node.js dependencies - ideal if you already have Python installed
- **C#**: Perfect if you're working in a .NET environment

Each implementation is self-contained with its own dependencies. The Python implementation uses PyMarkdown, which is a pure Python markdown linter.

## Requirements

Each implementation has its own requirements:

- **Python**: Python 3.10+ only
- **C#**: .NET runtime

See each implementation's README for specific setup instructions.
