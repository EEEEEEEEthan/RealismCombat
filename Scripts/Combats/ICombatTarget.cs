/// <summary>
///     战斗中可以被选择与结算的目标接口
/// </summary>
public interface ICombatTarget
{
	/// <summary>
	///     目标在日志或界面上的名称
	/// </summary>
	string Name { get; }
	/// <summary>
	///     目标的生命值属性
	/// </summary>
	PropertyInt HitPoint { get; }
	/// <summary>
	///     目标是否仍具备有效状态
	/// </summary>
	bool Available { get; }
}
