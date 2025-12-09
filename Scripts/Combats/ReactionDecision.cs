using System;
/// <summary>
///     战斗反应的类型
/// </summary>
public enum ReactionTypeCode
{
	/// <summary>
	///     不进行额外反应
	/// </summary>
	None,
	/// <summary>
	///     使用防御目标格挡
	/// </summary>
	Block,
	/// <summary>
	///     通过位移闪避攻击
	/// </summary>
	Dodge,
}
/// <summary>
///     战斗反应的决策结果
/// </summary>
public readonly struct ReactionDecision
{
	/// <summary>
	///     创建一个格挡决策
	/// </summary>
	public static ReactionDecision CreateBlock(ICombatTarget target)
	{
		ArgumentNullException.ThrowIfNull(target);
		return new(ReactionTypeCode.Block, target);
	}
	/// <summary>
	///     创建一个闪避决策
	/// </summary>
	public static ReactionDecision CreateDodge() => new(ReactionTypeCode.Dodge, null);
	/// <summary>
	///     创建一个承受决策
	/// </summary>
	public static ReactionDecision CreateEndure() => new(ReactionTypeCode.None, null);
	/// <summary>
	///     选择的反应类型
	/// </summary>
	public readonly ReactionTypeCode type;
	/// <summary>
	///     格挡时使用的目标
	/// </summary>
	public readonly ICombatTarget? blockTarget;
	ReactionDecision(ReactionTypeCode type, ICombatTarget? blockTarget)
	{
		this.type = type;
		this.blockTarget = blockTarget;
	}
}
