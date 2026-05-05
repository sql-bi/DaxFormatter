using Dax.Formatter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSingleton<IDaxFormatterClient, DaxFormatterClient>();
builder.Services
    .AddMcpServer(ConfigureMcpServerOptions)
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

static void ConfigureMcpServerOptions(ModelContextProtocol.Server.McpServerOptions options)
{
    options.ServerInfo = new ModelContextProtocol.Protocol.Implementation
    {
        Name = "dax-formatter-mcp",
        Version = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0",
        WebsiteUrl = "https://www.daxformatter.com"
    };
    options.ServerInstructions = """
        # DAX Formatter MCP Server

        ## Core Purpose

        This server formats DAX (Data Analysis Expressions) source code for Microsoft
        Power BI, Analysis Services, and Tabular models. It is the canonical wrapper
        for the SQLBI daxformatter.com web service.

        ## Strict Behavioral Rules

        ### 1. Tool Selection

        - No bypass: when DAX needs formatting, you MUST use this server's tools.
          NEVER call the daxformatter.com HTTP endpoint directly.
          This server is the single canonical channel for DAX formatting.

        ### 2. Code Integrity

        - Pass input as-is: the user's DAX is the authoritative source. You MUST
          send it to the tool exactly as provided without altering the string in any way.
          You MAY propose or apply changes only when the user has explicitly asked you to do so.

        ### 3. Surface Errors

        - Relay verbatim: if the formatter reports errors, you MUST pass them to
          the user exactly as received including all details. They are diagnostic
          information the user needs to fix the code.
        """;
}