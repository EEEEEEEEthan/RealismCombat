using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace RealismCombat.McpServer;
static partial class Program
{
	public static class SettingKeys
	{
		public const string godotPath = "godot";
	}
	public static readonly string projectRoot = ProjectRoot;
	public static readonly IReadOnlyDictionary<string, string> settings;
	static string ProjectRoot
	{
		get
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
	}
	static Program()
	{
		settings = ensureLocalSettings();
		return;
		static Dictionary<string, string> ensureLocalSettings()
		{
			var result = new Dictionary<string, string>();
			var settingsPath = Path.Combine(projectRoot, ".local.settings");
			if (!File.Exists(settingsPath)) File.Create(settingsPath);
			var regex = ConfigRegex();
			foreach (var line in File.ReadLines(settingsPath))
			{
				var match = regex.Match(line);
				if (match.Success)
				{
					var key = match.Groups[1].Value.Trim();
					var value = match.Groups[2].Value.Trim();
					result[key] = value;
				}
			}
			if (!result.ContainsKey("godot")) File.AppendAllLines(settingsPath, ["godot = GODOT.exe",]);
			return result;
		}
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
	[GeneratedRegex(@"(\S+)\s*=\s*(.+)")] private static partial Regex ConfigRegex();
}
