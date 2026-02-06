using System.Diagnostics;
using System.Text.Json;
using ModelContextProtocol.NET;
using ModelContextProtocol.NET.Server;

namespace MarkdownLintMCP;

/// <summary>
/// MCP Server for Markdownlint-CLI2
/// Provides markdown linting capabilities through the Model Context Protocol
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        var server = new MCPServer(
            serverName: "markdownlint",
            serverVersion: "1.0.0"
        );

        // Register tools
        server.AddTool(
            name: "lint_markdown",
            description: "Lint a markdown file using markdownlint. Returns a list of linting errors and warnings with line numbers, rule names, and descriptions.",
            inputSchema: new
            {
                type = "object",
                properties = new
                {
                    file_path = new
                    {
                        type = "string",
                        description = "Path to the markdown file to lint (absolute or relative path)"
                    },
                    config_path = new
                    {
                        type = "string",
                        description = "Optional path to a .markdownlint.json config file"
                    }
                },
                required = new[] { "file_path" }
            },
            handler: async (arguments) =>
            {
                return await LintMarkdown(arguments);
            }
        );

        server.AddTool(
            name: "fix_markdown",
            description: "Automatically fix markdown linting issues where possible. Returns the result of the fix operation.",
            inputSchema: new
            {
                type = "object",
                properties = new
                {
                    file_path = new
                    {
                        type = "string",
                        description = "Path to the markdown file to fix (absolute or relative path)"
                    },
                    config_path = new
                    {
                        type = "string",
                        description = "Optional path to a .markdownlint.json config file"
                    }
                },
                required = new[] { "file_path" }
            },
            handler: async (arguments) =>
            {
                return await FixMarkdown(arguments);
            }
        );

        // Run the server
        await server.RunAsync();
    }

    /// <summary>
    /// Find a .markdownlint.json config file by searching:
    /// 1. Same directory as the file
    /// 2. Parent directories up to the root
    /// </summary>
    /// <param name="filePath">Path to the markdown file</param>
    /// <returns>Path to the config file if found, null otherwise</returns>
    private static string? FindConfigFile(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var currentDir = fileInfo.Directory;

        var configNames = new[] { ".markdownlint.json", ".markdownlint.jsonc", ".markdownlintrc" };

        while (currentDir != null)
        {
            foreach (var configName in configNames)
            {
                var configPath = Path.Combine(currentDir.FullName, configName);
                if (File.Exists(configPath))
                {
                    return configPath;
                }
            }
            currentDir = currentDir.Parent;
        }

        return null;
    }

    private static async Task<string> LintMarkdown(JsonElement arguments)
    {
        try
        {
            if (!arguments.TryGetProperty("file_path", out var filePathElement))
            {
                throw new ArgumentException("file_path is required");
            }

            var filePath = filePathElement.GetString();
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("file_path cannot be empty");
            }

            // Check if markdownlint-cli2 is installed
            if (!IsMarkdownlintInstalled())
            {
                return "Error: markdownlint-cli2 is not installed. Please install it with: npm install -g markdownlint-cli2";
            }

            // Build command arguments
            var markdownlintArgs = new List<string> { filePath };

            // Auto-discover config file if not explicitly provided
            string? configPath = null;
            if (arguments.TryGetProperty("config_path", out var configPathElement))
            {
                configPath = configPathElement.GetString();
            }

            if (string.IsNullOrEmpty(configPath))
            {
                configPath = FindConfigFile(filePath);
            }

            // Add config if found or provided
            if (!string.IsNullOrEmpty(configPath))
            {
                markdownlintArgs.Add("--config");
                markdownlintArgs.Add(configPath);
            }

            // Run markdownlint-cli2
            var (exitCode, output, error) = await RunMarkdownlintAsync(markdownlintArgs.ToArray());

            if (exitCode == 0)
            {
                return $"✓ No linting errors found in {filePath}";
            }
            else
            {
                var result = !string.IsNullOrWhiteSpace(output) ? output : error;
                return $"Linting errors found in {filePath}:\n\n{result}";
            }
        }
        catch (Exception ex)
        {
            return $"Error linting markdown: {ex.Message}";
        }
    }

    private static async Task<string> FixMarkdown(JsonElement arguments)
    {
        try
        {
            if (!arguments.TryGetProperty("file_path", out var filePathElement))
            {
                throw new ArgumentException("file_path is required");
            }

            var filePath = filePathElement.GetString();
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("file_path cannot be empty");
            }

            // Check if markdownlint-cli2 is installed
            if (!IsMarkdownlintInstalled())
            {
                return "Error: markdownlint-cli2 is not installed. Please install it with: npm install -g markdownlint-cli2";
            }

            // Build command arguments
            var markdownlintArgs = new List<string> { filePath, "--fix" };

            // Auto-discover config file if not explicitly provided
            string? configPath = null;
            if (arguments.TryGetProperty("config_path", out var configPathElement))
            {
                configPath = configPathElement.GetString();
            }

            if (string.IsNullOrEmpty(configPath))
            {
                configPath = FindConfigFile(filePath);
            }

            // Add config if found or provided
            if (!string.IsNullOrEmpty(configPath))
            {
                markdownlintArgs.Add("--config");
                markdownlintArgs.Add(configPath);
            }

            // Run markdownlint-cli2 with --fix
            var (exitCode, output, error) = await RunMarkdownlintAsync(markdownlintArgs.ToArray());

            if (exitCode == 0)
            {
                var result = !string.IsNullOrWhiteSpace(output) ? $"\n\n{output}" : "";
                return $"✓ Fixed linting issues in {filePath}{result}";
            }
            else
            {
                var result = !string.IsNullOrWhiteSpace(output) ? output : error;
                return $"Fixed some issues in {filePath}, but some remain:\n\n{result}";
            }
        }
        catch (Exception ex)
        {
            return $"Error fixing markdown: {ex.Message}";
        }
    }

    private static bool IsMarkdownlintInstalled()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "markdownlint-cli2",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<(int exitCode, string output, string error)> RunMarkdownlintAsync(string[] arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "markdownlint-cli2",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var output = await outputTask;
        var error = await errorTask;

        return (process.ExitCode, output, error);
    }
}
