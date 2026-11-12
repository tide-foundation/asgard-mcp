using ModelContextProtocol.Protocol;

namespace Asgard.Mcp.Tests;

internal static class CallToolResultExtensions
{
    /// <summary>The concatenated text of every text block in a tool result.</summary>
    public static string Text(this CallToolResult result) =>
        string.Concat(result.Content.OfType<TextContentBlock>().Select(c => c.Text));
}
