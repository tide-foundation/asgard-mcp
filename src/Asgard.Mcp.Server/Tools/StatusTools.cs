using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Asgard.Mcp.Server.Tools;

/// <summary>
/// Example tool, useful for checking that an agent is connected. Add your own tool classes to this
/// folder: mark the class with [McpServerToolType] and each tool method with [McpServerTool].
/// The [Description] text is what the model reads to decide when to call the tool.
/// Stateless tools like this one can be static. Tools that need services use instance methods on a
/// non-static class and take the services through its constructor.
/// </summary>
[McpServerToolType]
public static class StatusTools
{
    [McpServerTool(Name = "asgard_status", Title = "Asgard status", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Reports that the asgard-mcp server is running and its version. " +
                 "Use this when the user asks whether Asgard is connected or working.")]
    public static string GetStatus() =>
        $"asgard-mcp {AsgardMcpServiceCollectionExtensions.ServerVersion} is running.";
}
