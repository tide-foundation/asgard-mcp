using Asgard.Mcp.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// stdout carries the MCP protocol. Anything else written there (Console.WriteLine, child
// process output) corrupts the stream and the client disconnects. Send all logs to stderr.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services
    .AddAsgardMcpServer()
    .WithStdioServerTransport();

await builder.Build().RunAsync();
