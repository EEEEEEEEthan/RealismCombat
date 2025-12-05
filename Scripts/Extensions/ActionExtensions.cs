using System;
public static partial class Extensions
{
	public static void TryInvoke<T>(this Action<T>? @this, T arg)
	{
		try
		{
			@this?.Invoke(arg);
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
	}
}
