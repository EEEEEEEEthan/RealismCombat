using System;
namespace RealismCombat.Extensions;
public static partial class Extensions
{
	public static void TryInvoke(this Action @this)
	{
		try
		{
			@this();
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
	}
}
