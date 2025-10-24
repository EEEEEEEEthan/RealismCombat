using System;
using System.Diagnostics;
using Godot;
using RealismCombat.Game.Extensions;
namespace RealismCombat.Game;
public static class Log
{
	public static event Action<string>? OnLog;
	public static event Action<string>? OnError;
	public static void Print(params object[] objects)
	{
		var text = $"{string.Join(separator: " ", values: objects)}";
		GD.Print(text);
		OnLog?.TryInvoke(text);
	}
	public static void PrintE(Exception e)
	{
		var text = $"[Exception] {e.GetType()}{e.Message}\n{e.StackTrace}";
		GD.PrintErr(text);
		OnError?.TryInvoke(text);
	}
	public static void PrintE(object message)
	{
		var stackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true).ToString();
		var text = $"[Error] {message}\n{stackTrace}";
		GD.PrintErr(text);
		OnError?.TryInvoke(text);
	}
}
