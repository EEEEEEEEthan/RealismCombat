using System;
using Godot;
namespace RealismCombat;
/// <summary>
///     日志工具类
/// </summary>
public static class Log
{
	public static void Print(params object[] args) => GD.Print(string.Join(" ", args));
	public static void PrintErr(params object[] args) => GD.PrintErr(string.Join(" ", args));
	public static void PrintWarn(params object[] args) => GD.PushWarning(string.Join(" ", args));
	public static void PrintException(Exception ex)
	{
		GD.PrintErr($"异常: {ex.GetType().Name}");
		GD.PrintErr($"消息: {ex.Message}");
		GD.PrintErr($"堆栈: {ex.StackTrace}");
	}
}
