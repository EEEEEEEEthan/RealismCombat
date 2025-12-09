using System.Collections;
using System.Collections.Generic;
public static partial class Extensions
{
	extension<T>((T, T) @this)
	{
		public IEnumerator<T> GetEnumerator()
		{
			yield return @this.Item1;
			yield return @this.Item2;
		}
	}
}
