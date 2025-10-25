using System.Text;
namespace RealismCombat.McpServer;
/// <summary>
///     MCP服务器日志记录器
/// </summary>
public static class Logger
{
	static readonly object logLock = new();
	static readonly StreamWriter? logWriter;
	static Logger()
	{
		var projectRoot = GetProjectRoot();
		var logDir = Path.Combine(projectRoot, ".logs");
		try
		{
			Directory.CreateDirectory(logDir);
		}
		catch
		{
			// ignored
		}
		var logFilePath1 = Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_mcp_server.log");
		try
		{
			logWriter = new(new FileStream(logFilePath1, FileMode.Append, FileAccess.Write, FileShare.Read), new UTF8Encoding(false))
			{
				AutoFlush = true,
			};
			Log("MCP服务器日志已初始化");
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"[McpLogger] 无法初始化日志文件: {ex.Message}");
		}
	}
	public static void Log(string message)
	{
		var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		var logMessage = $"[{timestamp}] {message}";
		lock (logLock)
		{
			try
			{
				logWriter?.WriteLine(logMessage);
			}
			catch
			{
				// ignored
			}
		}
	}
	public static void LogError(string message, Exception? ex = null)
	{
		var errorMessage = ex is null ? message : $"{message}: {ex.Message}";
		Log($"ERROR: {errorMessage}");
	}
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
}
