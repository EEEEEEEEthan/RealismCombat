using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace RealismCombat.McpServer;
static class Program
{
	static async Task Main(string[] args)
	{
		var builder = Host.CreateApplicationBuilder(args);
		builder.Logging.AddConsole(consoleLogOptions => { consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace; });
		builder
			.Services
			.AddMcpServer()
			.WithStdioServerTransport()
			.WithToolsFromAssembly();
		await builder.Build().RunAsync();
	}
}
