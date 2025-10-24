// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
using System.ComponentModel;
using ModelContextProtocol.Server;
namespace RealismCombat.McpServer;
[McpServerToolType]
static class SystemTools
{
	[McpServerTool, Description("hello"),]
	static Task<string> hello() => GameLauncher.SendCommand("hello");
	[McpServerTool, Description("start game"),]
	static string start_game() => GameLauncher.StartGame();
	[McpServerTool, Description("stop game"),]
	static Task<string> stop_game() => GameLauncher.SendCommand("system.shutdown");
}
