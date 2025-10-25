// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
using System.ComponentModel;
using ModelContextProtocol.Server;
namespace RealismCombat.McpServer;
[McpServerToolType]
static class SystemTools
{
	[McpServerTool, Description("start game"),]
	static string start_game() => GameClient.StartGame();
	[McpServerTool, Description("stop game"),]
	static Task<string> stop_game() => GameClient.SendCommand("system.shutdown", 3000);
}
[McpServerToolType]
static class GameTools
{
	[McpServerTool, Description("check current status"),]
	static Task<string> status() => GameClient.SendCommand("game.check_status", 3000);
	[McpServerTool, Description("start next combat"),]
	static Task<string> start_next_combat() => GameClient.SendCommand("game.start_next_combat", 3000);
}