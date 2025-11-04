using System;
namespace RealismCombat.Extensions;
public static class Extensions
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
