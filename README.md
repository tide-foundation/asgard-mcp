# asgard-mcp

Connect autonomous systems to the Tide Network.

asgard-mcp is a [Model Context Protocol](https://modelcontextprotocol.io) (MCP) server written in
.NET 10. AI agents (Claude Code, VS Code Copilot, Claude Desktop, Cursor, ...) start it locally over
stdio and call its tools.

> **Status: scaffold.** The server, tests and agent configuration work end to end. The tools are
> examples for testing: `asgard_status` and a simple calculator (`add`, `subtract`, `multiply`,
> `divide`).

## Quick start

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
dotnet build
dotnet test
```

**Use it from an agent.** This repo already contains config for Claude Code
([`.mcp.json`](.mcp.json)) and VS Code ([`.vscode/mcp.json`](.vscode/mcp.json)). Open the repo in
either one, approve the `asgard` server, and ask *"Is Asgard connected?"*.

For other agents, other projects, published binaries and NuGet, see
**[docs/connecting-agents.md](docs/connecting-agents.md)**.

**Try it without an agent** (needs Node.js):

```bash
npx @modelcontextprotocol/inspector dotnet run --project src/Asgard.Mcp
```

## Repository layout

```
asgard-mcp/
├── Asgard.Mcp.slnx
├── global.json                    pins the .NET 10 SDK; selects Microsoft.Testing.Platform for dotnet test
├── Directory.Build.props          shared settings (net10.0, nullable, analyzers, warnings as errors, version)
├── Directory.Packages.props       every NuGet version, in one place (also pins transitive versions)
├── .editorconfig                  formatting and C# style rules, enforced in the build
├── .mcp.json                      Claude Code project config
├── .vscode/mcp.json               VS Code workspace config
├── docs/
│   └── connecting-agents.md       how to add the server to each agent
├── src/
│   ├── Asgard.Mcp.Server/         the MCP server: tools and services (no transport)
│   │   ├── AsgardMcpServiceCollectionExtensions.cs   AddAsgardMcpServer(): the single registration point
│   │   └── Tools/                 [McpServerToolType] classes
│   └── Asgard.Mcp/                stdio host: Program.cs + .NET tool packaging
│       └── .mcp/server.json       MCP registry metadata, shipped in the NuGet package
└── tests/
    └── Asgard.Mcp.Tests/          runs the server in-memory against a real MCP client
```

`Asgard.Mcp.Server` holds the server; the host only picks a transport:

```csharp
builder.Services
    .AddAsgardMcpServer()          // tools + services
    .WithStdioServerTransport();   // transport chosen by the host
```

This keeps tools testable without starting a process. An HTTP host can be added later
(`.WithHttpTransport()` + `app.MapMcp()`) without touching them.

## Adding a tool

Create a class in `src/Asgard.Mcp.Server/Tools/`:

```csharp
[McpServerToolType]
public sealed class MyTools(IMyService service)   // services are constructor-injected
{
    [McpServerTool(Name = "my_tool")]
    [Description("What it does, and when the agent should use it.")]
    public Task<string> MyTool(
        [Description("What this argument means")] string input,
        CancellationToken cancellationToken) => service.DoAsync(input, cancellationToken);
}
```

It's discovered automatically. Register any services it needs in `AddAsgardMcpServer()`.
A tool that needs no services can be a `static` method on a `static` class, like the examples in
`Tools/`. The build's analyzers ask for this (CA1822).

- Write `[Description]` text for the model: it's how the agent decides when to call the tool.
- Throw `McpException` for errors the agent should see. Its message is returned as a tool error.
  Other exceptions are reported without details.
- **Never write to stdout.** It carries the MCP protocol. Log through `ILogger`, which the host
  sends to stderr.

## Publishing

```bash
dotnet pack src/Asgard.Mcp -c Release -o artifacts/nupkgs
```

This produces a small root package plus one self-contained package per runtime (win-x64, win-arm64,
osx-arm64, linux-x64, linux-arm64, linux-musl-x64). Users then run it with
`dnx Tide.Asgard.Mcp --yes`.

Before the first publish:

- Confirm the package ID `Tide.Asgard.Mcp` in `src/Asgard.Mcp/Asgard.Mcp.csproj` (and in
  `.mcp/server.json`) is one you can own on NuGet.org.
- Keep the version in `Directory.Build.props` in sync with `src/Asgard.Mcp/.mcp/server.json`.
- Push with `dotnet nuget push artifacts/nupkgs/*.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json`.

## References

- [MCP C# SDK documentation](https://csharp.sdk.modelcontextprotocol.io/)
- [Build an MCP server with .NET (Microsoft Learn)](https://learn.microsoft.com/dotnet/ai/quickstarts/build-mcp-server)
- [MCP specification](https://modelcontextprotocol.io/specification)
