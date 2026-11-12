using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Asgard.Mcp.Server.Tools;

/// <summary>
/// Simple calculator tools for testing the server end to end.
/// </summary>
[McpServerToolType]
public static class CalculatorTools
{
    [McpServerTool(Name = "add", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Adds two numbers.")]
    public static double Add(
        [Description("First number")] double a,
        [Description("Second number")] double b) => a + b;

    [McpServerTool(Name = "subtract", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Subtracts the second number from the first.")]
    public static double Subtract(
        [Description("Number to subtract from")] double a,
        [Description("Number to subtract")] double b) => a - b;

    [McpServerTool(Name = "multiply", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Multiplies two numbers.")]
    public static double Multiply(
        [Description("First number")] double a,
        [Description("Second number")] double b) => a * b;

    [McpServerTool(Name = "divide", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Divides the first number by the second.")]
    public static double Divide(
        [Description("Dividend")] double a,
        [Description("Divisor (must not be zero)")] double b) =>
        b == 0 ? throw new McpException("Cannot divide by zero.") : a / b;
}
