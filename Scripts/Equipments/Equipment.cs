using RealismCombat.Combats;
namespace RealismCombat.Equipments;
/// <summary>
///     战斗中可以被选择的装备实体
/// </summary>
public abstract class Equipment(EquipmentType type, PropertyInt hitPoint) : ICombatTarget
{
	public readonly EquipmentType type = type;
	/// <summary>
	///     目标是否仍具备有效状态
	/// </summary>
	public bool Available => HitPoint.value > 0;
	/// <summary>
	///     装备的耐久属性
	/// </summary>
	public PropertyInt HitPoint { get; } = hitPoint;
	/// <summary>
	///     目标在日志或界面上的名称
	/// </summary>
	public abstract string Name { get; }
}
public class LongSword() : Equipment(EquipmentType.Arm, new(10, 10))
{
	public override string Name => "长剑";
}
