using System;
namespace RealismCombat.Data;
/// <summary>
///     伤害类型
/// </summary>
[Flags]
public enum DamageType : byte
{
	/// <summary>无伤害</summary>
	None = 0,
	/// <summary>钝击伤害 - 由钝器造成的冲击伤害</summary>
	Blunt = 1 << 0,
	/// <summary>劈砍伤害 - 由刀剑等武器的劈砍动作造成</summary>
	Slash = 1 << 1,
	/// <summary>穿刺伤害 - 由尖锐武器刺入造成</summary>
	Pierce = 1 << 2,
}
