using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using RealismCombat.McpServer.Extensions;
namespace RealismCombat.McpServer;
/// <summary>
///     MCP服务器日志记录器
/// </summary>
public static class Log
{
	readonly struct Scope : IDisposable
	{
		readonly Action<string> onLog;
		public Scope(out StringBuilder builder)
		{
			builder = new();
			var copiedBuilder = builder;
			onLog = msg => copiedBuilder.AppendLine(msg);
			OnLog += onLog;
			OnError += onLog;
		}
		void IDisposable.Dispose()
		{
			OnError -= onLog;
			OnLog -= onLog;
		}
	}
	static readonly object logLock = new();
	static readonly StreamWriter? logWriter;
	public static event Action<string>? OnLog;
	public static event Action<string>? OnError;
	static Log()
	{
		var logDir = Path.Combine(Program.projectRoot, ".logs");
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
			logWriter = new(new FileStream(logFilePath1, FileMode.Create, FileAccess.Write, FileShare.Read), new UTF8Encoding(false))
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
	public static IDisposable BeginScope(out StringBuilder builder) => new Scope(out builder);
	public static void Print(params object[] objects)
	{
		var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		var logMessage = $"[{timestamp}] {string.Join(" ", objects)}";
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
		OnLog?.TryInvoke(logMessage);
	}
	public static void PrintError(params object[] objects)
	{
		var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		var stackTrace = new StackTrace(1, true);
		var logMessage = $"[{timestamp}][ERROR] {string.Join(" ", objects)}\n{stackTrace}";
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
		OnError?.TryInvoke<string>(logMessage);
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
