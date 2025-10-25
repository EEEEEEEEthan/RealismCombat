using System;
using System.Collections.Generic;
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
				case ShutdownCommand.name:
					new ShutdownCommand(gameRoot).Execute(arguments);
					break;
				case CheckStatusCommand.name:
					new CheckStatusCommand(gameRoot).Execute(arguments);
					break;
				case StartCombatCommand.name:
					new StartCombatCommand(gameRoot).Execute(arguments);
					break;
				default:
					Log.Print($"未知指令{cmd}");
					gameRoot.mcpHandler?.McpCheckPoint();
					break;
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			gameRoot.mcpHandler?.McpCheckPoint();
		}
	}
}
