using System.Text.RegularExpressions;
namespace RealismCombat.Extensions;
static partial class Extensions
{
	public static bool TryMatch(this Regex @this, string text, out Match match)
	{
		match = @this.Match(text);
		return match.Success;
	}
}
