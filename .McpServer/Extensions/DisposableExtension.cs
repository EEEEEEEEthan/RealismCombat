using System;
namespace RealismCombat.McpServer.Extensions;
public static partial class Extensions
{
	public static void TryDispose(this IDisposable @this)
	{
		try
		{
			@this.Dispose();
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
	}
}
