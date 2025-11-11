using System;
namespace RealismCombat.Equipments;
/// <summary>
///     装备类型掩码
/// </summary>
[Flags]
public enum EquipmentType
{
	/// <summary>
	///     武器类型装备
	/// </summary>
	Arm = 1 << 0,
}
