namespace RealismCombat.Extensions;
public static partial class Extensions
{
	public static float Remapped(this float @this, float fromMin, float fromMax, float toMin, float toMax) =>
		toMin + (@this - fromMin) * (toMax - toMin) / (fromMax - fromMin);
}
