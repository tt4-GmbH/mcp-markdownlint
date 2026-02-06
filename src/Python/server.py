#!/usr/bin/env python3
"""
MCP Server for PyMarkdown
Provides markdown linting capabilities through the Model Context Protocol
Uses pymarkdownlnt for pure Python markdown linting
"""

import asyncio
import json
import tempfile
from pathlib import Path
from io import StringIO
import sys
from mcp.server.models import InitializationOptions
import mcp.types as types
from mcp.server import NotificationOptions, Server
import mcp.server.stdio

try:
    from pymarkdown.api import PyMarkdownApi, PyMarkdownApiException
except ImportError:
    PyMarkdownApi = None
    PyMarkdownApiException = Exception

# Create server instance
server = Server("markdownlint")


def find_config_file(file_path: str) -> str | None:
    """
    Find a .markdownlint.json config file by searching:
    1. Same directory as the file
    2. Parent directories up to the root

    Returns the path to the config file if found, None otherwise.
    """
    file_path_obj = Path(file_path).resolve()

    # Start from the file's directory
    current_dir = file_path_obj.parent if file_path_obj.is_file() else file_path_obj

    # Search up the directory tree
    config_names = ['.markdownlint.json', '.markdownlint.jsonc', '.markdownlintrc']

    while current_dir != current_dir.parent:  # Stop at root
        for config_name in config_names:
            config_path = current_dir / config_name
            if config_path.exists():
                return str(config_path)
        current_dir = current_dir.parent

    return None


@server.list_tools()
async def handle_list_tools() -> list[types.Tool]:
    """List available tools."""
    return [
        types.Tool(
            name="lint_markdown",
            description="Lint a markdown file using markdownlint. Returns a list of linting errors and warnings with line numbers, rule names, and descriptions.",
            inputSchema={
                "type": "object",
                "properties": {
                    "file_path": {
                        "type": "string",
                        "description": "Path to the markdown file to lint (absolute or relative path)",
                    },
                    "config_path": {
                        "type": "string",
                        "description": "Optional path to a .markdownlint.json config file",
                    },
                },
                "required": ["file_path"],
            },
        ),
        types.Tool(
            name="fix_markdown",
            description="Automatically fix markdown linting issues where possible. Returns the result of the fix operation.",
            inputSchema={
                "type": "object",
                "properties": {
                    "file_path": {
                        "type": "string",
                        "description": "Path to the markdown file to fix (absolute or relative path)",
                    },
                    "config_path": {
                        "type": "string",
                        "description": "Optional path to a .markdownlint.json config file",
                    },
                },
                "required": ["file_path"],
            },
        ),
    ]


@server.call_tool()
async def handle_call_tool(
    name: str, arguments: dict | None
) -> list[types.TextContent | types.ImageContent | types.EmbeddedResource]:
    """Handle tool execution requests."""

    if not arguments:
        raise ValueError("Missing arguments")

    if PyMarkdownApi is None:
        return [
            types.TextContent(
                type="text",
                text="Error: pymarkdownlnt is not installed. Please install it with: pip install pymarkdownlnt"
            )
        ]

    if name == "lint_markdown":
        file_path = arguments.get("file_path")
        config_path = arguments.get("config_path")

        if not file_path:
            raise ValueError("file_path is required")

        # Resolve to absolute path to avoid working directory issues
        file_path = str(Path(file_path).resolve())

        if not Path(file_path).exists():
            return [
                types.TextContent(
                    type="text",
                    text=f"Error: File not found: {file_path}"
                )
            ]

        try:
            # Create PyMarkdown API instance
            api = PyMarkdownApi()

            # Auto-discover config file if not explicitly provided
            if not config_path:
                config_path = find_config_file(file_path)

            # Configure with config file if found or provided
            if config_path:
                api = api.configuration_file_path(config_path)

            # Capture output
            output_buffer = StringIO()
            original_stdout = sys.stdout
            sys.stdout = output_buffer

            try:
                # Scan the file
                scan_result = api.scan_path(file_path)

                # Restore stdout
                sys.stdout = original_stdout
                output = output_buffer.getvalue()

                if not scan_result.scan_failures:
                    return [
                        types.TextContent(
                            type="text",
                            text=f"✓ No linting errors found in {file_path}"
                        )
                    ]
                else:
                    return [
                        types.TextContent(
                            type="text",
                            text=f"Linting errors found in {file_path}:\n\n{output if output else 'Issues found but no detailed output.'}"
                        )
                    ]
            finally:
                sys.stdout = original_stdout

        except PyMarkdownApiException as e:
            return [
                types.TextContent(
                    type="text",
                    text=f"Error linting {file_path}: {str(e)}"
                )
            ]
        except Exception as e:
            return [
                types.TextContent(
                    type="text",
                    text=f"Error running pymarkdown: {str(e)}"
                )
            ]

    elif name == "fix_markdown":
        file_path = arguments.get("file_path")
        config_path = arguments.get("config_path")

        if not file_path:
            raise ValueError("file_path is required")

        # Resolve to absolute path to avoid working directory issues
        file_path = str(Path(file_path).resolve())

        if not Path(file_path).exists():
            return [
                types.TextContent(
                    type="text",
                    text=f"Error: File not found: {file_path}"
                )
            ]

        try:
            # Create PyMarkdown API instance
            api = PyMarkdownApi()

            # Auto-discover config file if not explicitly provided
            if not config_path:
                config_path = find_config_file(file_path)

            # Configure with config file if found or provided
            if config_path:
                api = api.configuration_file_path(config_path)

            # Capture output
            output_buffer = StringIO()
            original_stdout = sys.stdout
            sys.stdout = output_buffer

            try:
                # Fix the file
                fix_result = api.fix_path(file_path)

                # Restore stdout
                sys.stdout = original_stdout
                output = output_buffer.getvalue()

                if fix_result.files_fixed:
                    return [
                        types.TextContent(
                            type="text",
                            text=f"✓ Fixed linting issues in {file_path}\n\n{output if output else 'All auto-fixable issues have been resolved.'}"
                        )
                    ]
                else:
                    return [
                        types.TextContent(
                            type="text",
                            text=f"No auto-fixable issues found in {file_path}\n\n{output if output else 'File may already be compliant or issues require manual fixing.'}"
                        )
                    ]
            finally:
                sys.stdout = original_stdout

        except PyMarkdownApiException as e:
            return [
                types.TextContent(
                    type="text",
                    text=f"Error fixing {file_path}: {str(e)}"
                )
            ]
        except Exception as e:
            return [
                types.TextContent(
                    type="text",
                    text=f"Error running pymarkdown fix: {str(e)}"
                )
            ]

    else:
        raise ValueError(f"Unknown tool: {name}")


async def main():
    """Main entry point for the server."""
    async with mcp.server.stdio.stdio_server() as (read_stream, write_stream):
        await server.run(
            read_stream,
            write_stream,
            InitializationOptions(
                server_name="markdownlint",
                server_version="1.0.0",
                capabilities=server.get_capabilities(
                    notification_options=NotificationOptions(),
                    experimental_capabilities={},
                ),
            ),
        )


if __name__ == "__main__":
    asyncio.run(main())
