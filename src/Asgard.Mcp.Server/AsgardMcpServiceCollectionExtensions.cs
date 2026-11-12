using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;

namespace Asgard.Mcp.Server;

public static class AsgardMcpServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Asgard MCP server (tools and the services they use) without a transport.
    /// Callers add one afterwards, e.g. <c>.WithStdioServerTransport()</c>.
    /// </summary>
    public static IMcpServerBuilder AddAsgardMcpServer(this IServiceCollection services)
    {
        // Register services your tools depend on here; tools receive them by constructor injection.

        return services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new Implementation
                {
                    Name = "asgard-mcp",
                    Title = "Asgard",
                    Version = ServerVersion,
                };

                // Optional guidance sent to the agent during the handshake; clients that support it
                // add it to the model's context. Use it for cross-tool guidance, not per-tool docs.
                // options.ServerInstructions = "...";
            })
            // Tools live in this library, not in the host, so pass this assembly explicitly.
            // Prompts and resources work the same way: WithPromptsFromAssembly / WithResourcesFromAssembly.
            .WithToolsFromAssembly(typeof(AsgardMcpServiceCollectionExtensions).Assembly);
    }

    public static string ServerVersion { get; } =
        typeof(AsgardMcpServiceCollectionExtensions).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            .Split('+')[0] // drop the "+<commit>" suffix the SDK appends
        ?? "0.0.0";
}
