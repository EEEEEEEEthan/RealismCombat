// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
using System.ComponentModel;
using ModelContextProtocol.Server;
namespace RealismCombat.McpServer
{
	[McpServerToolType]
	static class SystemTools
	{
		[McpServerTool, Description("hello"),] static Task<string> hello() => Task.FromResult("hello");
	}
}
