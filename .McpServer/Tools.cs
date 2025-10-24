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
	static Task<string> hello() => GameClient.SendCommand("hello", 3000);
	[McpServerTool, Description("start game"),]
	static string start_game() => GameClient.StartGame();
	[McpServerTool, Description("stop game"),]
	static Task<string> stop_game() => GameClient.SendCommand("system.shutdown", 3000);
}
