namespace RealismCombat.McpServer.Extensions;
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
	public static void TryInvoke<T>(this Action<T> @this, T arg)
	{
		try
		{
			@this(arg);
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
	}
}
