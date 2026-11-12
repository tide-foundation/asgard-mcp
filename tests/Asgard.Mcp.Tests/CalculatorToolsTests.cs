namespace Asgard.Mcp.Tests;

public sealed class CalculatorToolsTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("add", 6, 3, "9")]
    [InlineData("subtract", 6, 3, "3")]
    [InlineData("multiply", 6, 3, "18")]
    [InlineData("divide", 6, 3, "2")]
    [InlineData("divide", 1, 4, "0.25")]
    public async Task Calculates(string tool, double a, double b, string expected)
    {
        await using var server = await InMemoryMcpServer.StartAsync(cancellationToken: Ct);

        var result = await server.CallToolAsync(tool, new() { ["a"] = a, ["b"] = b });

        Assert.NotEqual(true, result.IsError);
        Assert.Equal(expected, result.Text());
    }

    [Fact]
    public async Task Divide_by_zero_returns_a_tool_error()
    {
        await using var server = await InMemoryMcpServer.StartAsync(cancellationToken: Ct);

        var result = await server.CallToolAsync("divide", new() { ["a"] = 1, ["b"] = 0 });

        Assert.True(result.IsError);
        Assert.Contains("Cannot divide by zero", result.Text());
    }
}
