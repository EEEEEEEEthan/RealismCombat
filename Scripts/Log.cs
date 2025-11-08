using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
namespace RealismCombat;
/// <summary>
///     日志工具类
/// </summary>
public static class Log
{
	static string Timestamp => $"[{DateTime.Now:HH:mm:ss}]";
	public static event Action<string>? OnLog;
	public static event Action<string>? OnLogError;
	public static event Action<string>? OnLogWarning;
	public static void Print(params object[] args)
	{
		var message = string.Join(" ", args);
		GD.Print($"{Timestamp} {message}");
		OnLog?.Invoke(message);
	}
	public static void PrintError(params object[] args)
	{
		var message = string.Join(" ", args);
		var stackTrace = new StackTrace(1, true);
		var fullMessage = $"{message}\n调用栈:\n{stackTrace}";
		GD.PrintErr($"{Timestamp} {fullMessage}");
		OnLogError?.Invoke(fullMessage);
	}
	public static void PrintWarning(params object[] args)
	{
		var message = string.Join(" ", args);
		GD.PushWarning($"{Timestamp} {message}");
		OnLogWarning?.Invoke(message);
	}
	public static void PrintException(Exception ex)
	{
		var fullMessage = $"异常: {ex.GetType().Name}\n消息: {ex.Message}\n堆栈: {ex.StackTrace}";
		GD.PrintErr($"{Timestamp} {fullMessage}");
		OnLogError?.Invoke(fullMessage);
	}
}
/// <summary>
///     日志监听器，用于收集日志
/// </summary>
public sealed class LogListener : IDisposable
{
	readonly List<string> logs = [];
	readonly object sync = new();
	bool disposed;
	bool isCollecting;
	public LogListener()
	{
		Log.OnLog += HandleLog;
		Log.OnLogError += HandleLogError;
		Log.OnLogWarning += HandleLogWarning;
	}
	public void StartCollecting()
	{
		lock (sync)
		{
			logs.Clear();
			isCollecting = true;
		}
	}
	public string StopCollecting()
	{
		lock (sync)
		{
			isCollecting = false;
			return string.Join("\n", logs);
		}
	}
	public void Clear()
	{
		lock (sync)
		{
			logs.Clear();
		}
	}
	public void Dispose()
	{
		if (disposed) return;
		disposed = true;
		Log.OnLog -= HandleLog;
		Log.OnLogError -= HandleLogError;
		Log.OnLogWarning -= HandleLogWarning;
	}
	void HandleLog(string message)
	{
		lock (sync)
		{
			if (isCollecting) logs.Add(message);
		}
	}
	void HandleLogError(string message)
	{
		lock (sync)
		{
			if (isCollecting) logs.Add($"[ERROR] {message}");
		}
	}
	void HandleLogWarning(string message)
	{
		lock (sync)
		{
			if (isCollecting) logs.Add($"[WARN] {message}");
		}
	}
}
