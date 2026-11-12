using System.IO.Pipelines;
using Asgard.Mcp.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Asgard.Mcp.Tests;

/// <summary>
/// Runs the Asgard MCP server in-process, with a pipe standing in for stdin/stdout, and
/// connects a real MCP client to it. This exercises the same registration the stdio host uses.
/// </summary>
internal sealed class InMemoryMcpServer : IAsyncDisposable
{
    private readonly IHost _host;

    public McpClient Client { get; }

    private InMemoryMcpServer(IHost host, McpClient client)
    {
        _host = host;
        Client = client;
    }

    public static async Task<InMemoryMcpServer> StartAsync(
        Action<IServiceCollection>? configureServices = null,
        CancellationToken cancellationToken = default)
    {
        var clientToServer = new Pipe();
        var serverToClient = new Pipe();

        var builder = Host.CreateEmptyApplicationBuilder(settings: null);
        configureServices?.Invoke(builder.Services); // e.g. register test doubles for services your tools use
        builder.Services
            .AddAsgardMcpServer()
            .WithStreamServerTransport(clientToServer.Reader.AsStream(), serverToClient.Writer.AsStream());

        var host = builder.Build();
        await host.StartAsync(cancellationToken);

        var client = await McpClient.CreateAsync(
            new StreamClientTransport(clientToServer.Writer.AsStream(), serverToClient.Reader.AsStream()),
            cancellationToken: cancellationToken);

        return new InMemoryMcpServer(host, client);
    }

    public async Task<CallToolResult> CallToolAsync(string name, Dictionary<string, object?>? arguments = null) =>
        await Client.CallToolAsync(name, arguments, cancellationToken: TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync()
    {
        await Client.DisposeAsync();
        await _host.StopAsync();
        _host.Dispose();
    }
}
