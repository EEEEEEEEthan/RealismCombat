using System;
using System.Diagnostics;
using System.IO;
using System.Text;
namespace RealismCombat;
/// <summary>
///     MCP服务器日志记录器
/// </summary>
public static class Log
{
	static readonly object logLock = new();
	static readonly StreamWriter? logWriter;
	static Log()
	{
		const string logDir = ".logs";
		try
		{
			Directory.CreateDirectory(logDir);
		}
		catch
		{
			// ignored
		}
		var logFilePath1 = Path.Combine(path1: logDir, path2: $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_mcp_server.log");
		try
		{
			logWriter = new(stream: new FileStream(path: logFilePath1, mode: FileMode.Create, access: FileAccess.Write, share: FileShare.Read),
				encoding: new UTF8Encoding(false))
			{
				AutoFlush = true,
			};
			Print("MCP服务器日志已初始化");
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"[McpLogger] 无法初始化日志文件: {ex.Message}");
		}
	}
	public static void Print(params object[] objects)
	{
		var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		var logMessage = $"[{timestamp}] {string.Join(separator: " ", values: objects)}";
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
	public static void PrintError(params object[] objects)
	{
		var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		var stackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true);
		var logMessage = $"[{timestamp}][ERROR] {string.Join(separator: " ", values: objects)}\n{stackTrace}";
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
	public static void PrintException(Exception e)
	{
		var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		var logMessage = $"[{timestamp}][EXCEPTION] {e.GetType()} {e.Message}\n{e.StackTrace}";
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
}
