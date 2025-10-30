namespace RealismCombat.Extensions;
public static partial class Extensions
{
	public static double Clamped(this double @this, double min, double max) => @this < min ? min : @this > max ? max : @this;
}
