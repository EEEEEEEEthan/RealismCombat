using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace RealismCombat.McpServer;
static class Program
{
	public static readonly string projectRoot = GetProjectRoot();
	static string GetProjectRoot()
	{
		const string file = "project.godot";
		var currentDirectory = Directory.GetCurrentDirectory();
		if (File.Exists(Path.Combine(currentDirectory, file))) return currentDirectory;
		var dir = new DirectoryInfo(currentDirectory);
		while (dir is not null)
		{
			if (File.Exists(Path.Combine(dir.FullName, file))) return dir.FullName;
			dir = dir.Parent;
		}
		return currentDirectory;
	}
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
