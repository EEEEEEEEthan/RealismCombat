using System;
using System.Collections.Generic;
using Godot;
using RealismCombat.Commands;
namespace RealismCombat;
public class CommandHandler(GameRoot gameRoot)
{
	public void Execute(string cmd)
	{
		try
		{
			var parts = cmd.Split(" ");
			var name = parts[0];
			var arguments = new Dictionary<string, string>();
			for (var i = 1; i < parts.Length - 1; i += 2) arguments[parts[i]] = parts[i + 1];
			switch (name)
			{
				case "system.shutdown":
					gameRoot.CallDeferred(GameRoot.MethodName._QuitGame);
					Log.Print("游戏即将关闭");
					gameRoot.mcpHandler?.McpCheckPoint();
					break;
				case "game.check_status":
					PrintStatus();
					gameRoot.mcpHandler?.McpCheckPoint();
					break;
				case StartCombatCommand.name:
					new StartCombatCommand(gameRoot).Execute(arguments);
					break;
				default:
					Log.Print($"未知指令{cmd}");
					break;
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			gameRoot.mcpHandler?.McpCheckPoint();
		}
	}
	void PrintStatus()
	{
		Log.Print("游戏运行状态:");
		Log.Print($"  运行时间: {gameRoot.TotalTime:F2}秒");
		Log.Print($"  总帧数: {gameRoot.FrameCount}");
		Log.Print($"  平均FPS: {(gameRoot.TotalTime > 0 ? gameRoot.FrameCount / gameRoot.TotalTime : 0):F2}");
		Log.Print($"  当前FPS: {Engine.GetFramesPerSecond()}");
		Log.Print($"  客户端已连接: {gameRoot.HadClientConnected}");
		if (gameRoot.mcpHandler is not null)
			Log.Print("  服务器状态: 运行中");
		else
			Log.Print("  服务器状态: 未启动");
	}
}
