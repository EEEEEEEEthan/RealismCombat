using System;
/// <summary>
///     反应成功率的计算结果
/// </summary>
public readonly struct ReactionChance(double dodgeChance, double blockChance)
{
	/// <summary>
	///     闪避成功率
	/// </summary>
	public double DodgeChance { get; } = dodgeChance;
	/// <summary>
	///     格挡成功率
	/// </summary>
	public double BlockChance { get; } = blockChance;
	public double HighestChance => Math.Max(DodgeChance, BlockChance);
}
/// <summary>
///     反应结算结果
/// </summary>
public readonly struct ReactionOutcome(ReactionTypeCode type, ICombatTarget? blockTarget, bool succeeded, double successChance)
{
	/// <summary>
	///     反应类型
	/// </summary>
	public ReactionTypeCode Type { get; } = type;
	/// <summary>
	///     成功格挡时使用的目标
	/// </summary>
	public ICombatTarget? BlockTarget { get; } = blockTarget;
	/// <summary>
	///     是否成功
	/// </summary>
	public bool Succeeded { get; } = succeeded; // todo: 删掉。BlockTarget != null 就表示成功格挡了
	/// <summary>
	///     本次判定的成功率
	/// </summary>
	public double SuccessChance { get; } = successChance;
}
/// <summary>
///     负责计算并结算闪避与格挡成功率
/// </summary>
public static class ReactionSuccessCalculator { }
