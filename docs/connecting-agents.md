# Connecting agents to the Asgard MCP server

asgard-mcp is a **local stdio** MCP server. You don't run it yourself and it doesn't open a
port. Your agent (Claude Code, VS Code, Claude Desktop, ...) starts it as a child process when
a session begins and exchanges JSON-RPC with it over stdin/stdout.

So "adding the server to an agent" always means one thing: telling the agent **which command
starts the server**. Every client below takes the same command in a slightly different file.

- [1. Choose how the server is launched](#1-choose-how-the-server-is-launched)
- [2. Add it to your agent](#2-add-it-to-your-agent)
  - [Claude Code](#claude-code)
  - [VS Code (GitHub Copilot agent mode)](#vs-code-github-copilot-agent-mode)
  - [Claude Desktop](#claude-desktop)
  - [Cursor](#cursor)
  - [OpenAI Codex CLI](#openai-codex-cli)
  - [Visual Studio](#visual-studio)
  - [Any other MCP client](#any-other-mcp-client)
- [3. Check that it works](#3-check-that-it-works)
- [4. Troubleshooting](#4-troubleshooting)

---

## 1. Choose how the server is launched

| Option | Command | Use it when |
|---|---|---|
| **A. From source** | `dotnet run --project <repo>/src/Asgard.Mcp` | Developing the server. Rebuilds on each start, so restart the server in your client to pick up changes. Needs the .NET 10 SDK. |
| **B. Published binary** | `<publish-dir>/Asgard.Mcp` (`Asgard.Mcp.exe` on Windows) | Using a fixed build without the SDK. Starts fastest. |
| **C. NuGet package via `dnx`** | `dnx Tide.Asgard.Mcp --yes` | Distributing to other people once the package is published. Needs the .NET 10 SDK. |

`<repo>` is the absolute path to your clone, e.g. `/home/you/asgard-mcp`. Use absolute paths in
user-level or global configs; a relative path is resolved against whatever directory the client
happens to start in.

**Option B: build the binary.** The output is a self-contained single file, so the target
machine doesn't need .NET installed.

```bash
dotnet publish src/Asgard.Mcp -c Release -r linux-x64 -o out/linux-x64
# other runtime IDs: win-x64, win-arm64, osx-arm64, linux-arm64, linux-musl-x64
```

**Option C: try the package before publishing it.** Pack into a local folder and point `dnx`
at it. A prerelease version must be named explicitly with `--version` (or pass `--prerelease`).

```bash
dotnet pack src/Asgard.Mcp -c Release -o artifacts/nupkgs
dnx Tide.Asgard.Mcp --version 0.1.0-beta --add-source ./artifacts/nupkgs --yes
```

The examples below use **option A**. To use B or C, replace `command` and `args` with that
option's command.

---

## 2. Add it to your agent

### Claude Code

**In this repo there is nothing to do.** The checked-in [`.mcp.json`](../.mcp.json) registers the
server at *project scope* for anyone who opens the repo:

```json
{
  "mcpServers": {
    "asgard": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "src/Asgard.Mcp"]
    }
  }
}
```

Start `claude` from the repo root, since the project path is relative. Claude Code asks you to
approve project-scoped servers the first time; accept it, then run `/mcp` to confirm `asgard`
is connected.

**To use it in other projects**, add it with the CLI:

```bash
# --scope user: available in all your projects
# --scope local (the default): only you, only the current project
claude mcp add --scope user asgard -- dotnet run --project /home/you/asgard-mcp/src/Asgard.Mcp
```

Everything after `--` is the launch command. Useful commands: `claude mcp list`,
`claude mcp get asgard`, `claude mcp remove asgard`. To pass configuration, add `-e KEY=value`
before the server name.

### VS Code (GitHub Copilot agent mode)

**In this repo there is nothing to do.** The checked-in [`.vscode/mcp.json`](../.vscode/mcp.json)
registers the server for the workspace. Note that VS Code uses a `servers` key, not
`mcpServers`:

```json
{
  "servers": {
    "asgard": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "${workspaceFolder}/src/Asgard.Mcp"]
    }
  }
}
```

Open Copilot Chat, switch to **Agent** mode, and start the server from the tools picker or with
the **MCP: List Servers** command. To use it in every workspace, run **MCP: Open User
Configuration** and add the same entry with an absolute path in place of `${workspaceFolder}`.

### Claude Desktop

Open **Settings → Developer → Edit Config**, or edit the file directly:

- macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`
- Windows: `%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "mcpServers": {
    "asgard": {
      "command": "dotnet",
      "args": ["run", "--project", "/Users/you/asgard-mcp/src/Asgard.Mcp"]
    }
  }
}
```

Fully quit and reopen Claude Desktop after editing; it only reads the file at startup.

**Windows with the repo in WSL:** Claude Desktop runs on Windows, so either launch the server
through `wsl`:

```json
{
  "mcpServers": {
    "asgard": {
      "command": "wsl",
      "args": ["-e", "dotnet", "run", "--project", "/home/you/asgard-mcp/src/Asgard.Mcp"]
    }
  }
}
```

or publish a Windows binary (`-r win-x64`) and point `command` at the `.exe`, which starts faster.

### Cursor

Create `.cursor/mcp.json` in a project, or `~/.cursor/mcp.json` for all projects:

```json
{
  "mcpServers": {
    "asgard": {
      "command": "dotnet",
      "args": ["run", "--project", "/home/you/asgard-mcp/src/Asgard.Mcp"]
    }
  }
}
```

The server's status is shown under **Cursor Settings → MCP**.

### OpenAI Codex CLI

Add to `~/.codex/config.toml`:

```toml
[mcp_servers.asgard]
command = "dotnet"
args = ["run", "--project", "/home/you/asgard-mcp/src/Asgard.Mcp"]
```

### Visual Studio

Visual Studio 2022 (17.14+) and later read MCP configuration from several locations, including
`.vscode/mcp.json`, so the checked-in VS Code file may already be picked up when you open the
solution. Use the tools picker in Copilot Chat's Agent mode to enable it. See
[Use MCP servers in Visual Studio](https://learn.microsoft.com/visualstudio/ide/mcp-servers)
for the full list of locations.

### Any other MCP client

Look for a "stdio" or "command" server type and provide:

- **command:** `dotnet`
- **args:** `run`, `--project`, `/absolute/path/to/asgard-mcp/src/Asgard.Mcp`
- **env:** none required yet

---

## 3. Check that it works

**From the agent:** ask *"Is Asgard connected?"*. The agent should call the `asgard_status`
tool and report the server version.

**Without an agent:** the [MCP Inspector](https://github.com/modelcontextprotocol/inspector) is a
web UI for calling a server's tools by hand. It needs Node.js.

```bash
npx @modelcontextprotocol/inspector dotnet run --project src/Asgard.Mcp
```

Open the URL it prints, click **Connect**, then use the **Tools** tab: **List Tools**, then run
`asgard_status`.

**Automated:** `dotnet test` runs the server in-memory against a real MCP client.

---

## 4. Troubleshooting

| Symptom | Likely cause and fix |
|---|---|
| Server shows "failed" or "disconnected" right after starting | Something wrote to **stdout**, which is reserved for the protocol. Never use `Console.WriteLine`; log through `ILogger` (already routed to stderr), and redirect stdout of any child process you start. |
| Times out on first start | `dotnet run` is restoring and building. Run `dotnet build` once first, or use the published binary (option B). In Claude Code, raising the `MCP_TIMEOUT` environment variable (milliseconds) gives servers longer to start. |
| `dotnet: command not found` | GUI clients (Claude Desktop, Cursor) often don't see your shell's `PATH`. Use the absolute path to `dotnet` (`which dotnet`), or use the published binary. |
| Works in Claude Code but not in another client | Check the key name: VS Code uses `servers`, most others use `mcpServers`. Check that paths are absolute. |
| Code changes don't show up | The client keeps the old process. Restart the server from the client (`/mcp` in Claude Code, **MCP: List Servers → Restart** in VS Code). |
| Need to see the server's logs | Logs go to stderr. Claude Code shows server errors under `/mcp` and with `claude --debug`; VS Code shows them in the **Output** panel for the server. |

---

## Later: remote (HTTP) clients

Web-based clients such as claude.ai connectors can't start local processes; they need an HTTP
server. When that's needed, add a second host project (e.g. `src/Asgard.Mcp.Http`) that calls
`AddAsgardMcpServer().WithHttpTransport()` and `app.MapMcp()`, and clients connect by URL:

```json
{ "mcpServers": { "asgard": { "type": "http", "url": "https://asgard.example.com/mcp" } } }
```

The tools in `Asgard.Mcp.Server` don't change. A remote server does need authentication, which
a local stdio server doesn't.
