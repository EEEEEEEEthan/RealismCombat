using System;
namespace RealismCombat.Extensions;
public static class ActionExtension
{
	public static void TryInvoke(this Action @this)
	{
		try
		{
			@this();
		}
		catch (Exception e)
		{
			Log.PrintE(e);
		}
	}
	public static void TryInvoke<T>(this Action<T> @this, T arg)
	{
		try
		{
			@this(arg);
		}
		catch (Exception e)
		{
			Log.PrintE(e);
		}
	}
}
