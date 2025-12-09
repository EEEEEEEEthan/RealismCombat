using System;
public static partial class Extensions
{
	extension(double @this)
	{
		public int RoundToInt() => (int)Math.Round(@this);
		public int FloorToInt() => (int)Math.Floor(@this);
		public int CeilToInt() => (int)Math.Ceiling(@this);
	}
}
