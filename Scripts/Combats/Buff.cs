namespace RealismCombat.Combats;
using RealismCombat.Characters;
/// <summary>
///     Buff类
/// </summary>
public class Buff
{
	public readonly BuffCode code;
	/// <summary>
	///     Buff的来源角色
	/// </summary>
	public readonly Character? source;
	public Buff(BuffCode code, Character? source = null)
	{
		this.code = code;
		this.source = source;
	}
}

