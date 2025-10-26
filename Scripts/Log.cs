using System;
using System.Diagnostics;
using System.Text;
using Godot;
using RealismCombat.Extensions;
namespace RealismCombat;
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
	public static event Action<string>? OnLog;
	public static event Action<string>? OnError;
	public static IDisposable BeginScope(out StringBuilder builder) => new Scope(out builder);
	public static void Print(params object[] objects)
	{
		var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		var logMessage = $"{string.Join(separator: " ", values: objects)}";
		GD.Print($"[{timestamp}]{logMessage}");
		OnLog?.TryInvoke(logMessage);
	}
	public static void PrintError(params object[] objects)
	{
		var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		var stackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true);
		var logMessage = $"[ERROR] {string.Join(separator: " ", values: objects)}\n{stackTrace}";
		GD.Print($"[{timestamp}]{logMessage}");
		OnError?.TryInvoke<string>(logMessage);
	}
	public static void PrintException(Exception e)
	{
		var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		var logMessage = $"[EXCEPTION] {e.GetType()} {e.Message}\n{e.StackTrace}";
		GD.Print($"[{timestamp}]{logMessage}");
		OnError?.TryInvoke<string>(logMessage);
	}
}
