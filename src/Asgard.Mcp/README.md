# Asgard MCP server

An MCP server that connects autonomous agents to the Tide Network. It runs locally over
stdio and is started by your MCP client.

## Use it from NuGet

With the .NET 10 SDK installed, add this to your client's MCP configuration:

```json
{
  "command": "dnx",
  "args": ["Tide.Asgard.Mcp", "--yes"]
}
```

For client-specific setup, see the repository's
[connecting agents guide](https://github.com/tide-foundation/asgard-mcp/blob/main/docs/connecting-agents.md).
