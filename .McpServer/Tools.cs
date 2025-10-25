// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
using System.ComponentModel;
using ModelContextProtocol.Server;
namespace RealismCombat.McpServer;
[McpServerToolType]
static class SystemTools
{
    static GameClient? client;
    public static GameClient? Client => client;
    [McpServerTool, Description("start game"),]
    static string start_game()
    {
        if (client != null) return $"ok: {client.Port}";
        try
        {
            client = new GameClient();
            return $"ok: {client.Port}";
        }
        catch (Exception e)
        {
            client?.Dispose();
            client = null;
            return $"error: {e.Message}";
        }
    }
    [McpServerTool, Description("stop game"),]
    static string stop_game()
    {
        try
        {
            var result = client is null ? "not running" : client.SendCommand("system.shutdown", 3000).GetAwaiter().GetResult();
            client?.Dispose();
            client = null;
            return result;
        }
        catch (Exception e)
        {
            client?.Dispose();
            client = null;
            return $"error: {e.Message}";
        }
    }
}
/*
[McpServerToolType]
static class GameTools
{
    [McpServerTool, Description("check current status"),]
    static Task<string> status()
    {
        if (SystemTools.Client is null) return Task.FromResult("未连接到游戏");
        return SystemTools.Client.SendCommand("game.check_status", 3000);
    }
    [McpServerTool, Description("start next combat"),]
    static Task<string> start_next_combat()
    {
        if (SystemTools.Client is null) return Task.FromResult("未连接到游戏");
        return SystemTools.Client.SendCommand("game.start_next_combat", 3000);
    }
}
*/