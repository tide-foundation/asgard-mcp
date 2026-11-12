namespace Asgard.Mcp.Tests;

public sealed class ServerTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Handshake_reports_server_name()
    {
        await using var server = await InMemoryMcpServer.StartAsync(cancellationToken: Ct);

        Assert.Equal("asgard-mcp", server.Client.ServerInfo?.Name);
    }

    [Fact]
    public async Task All_tools_have_descriptions()
    {
        await using var server = await InMemoryMcpServer.StartAsync(cancellationToken: Ct);

        var tools = await server.Client.ListToolsAsync(cancellationToken: Ct);

        Assert.Contains(tools, t => t.Name == "asgard_status");
        Assert.All(tools, t => Assert.False(string.IsNullOrWhiteSpace(t.Description)));
    }

    [Fact]
    public async Task Status_reports_version()
    {
        await using var server = await InMemoryMcpServer.StartAsync(cancellationToken: Ct);

        var result = await server.CallToolAsync("asgard_status");

        Assert.NotEqual(true, result.IsError);
        Assert.Contains("asgard-mcp", result.Text());
    }
}
