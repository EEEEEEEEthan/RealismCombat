using System.Collections.Generic;
/// <summary>
///     Buff拥有者接口
/// </summary>
public interface IBuffOwner
{
	/// <summary>
	///     获取所有Buff列表
	/// </summary>
	IReadOnlyList<Buff> Buffs { get; }
	/// <summary>
	///     添加Buff
	/// </summary>
	void AddBuff(Buff buff);
	/// <summary>
	///     移除Buff
	/// </summary>
	void RemoveBuff(Buff buff);
	/// <summary>
	///     检查是否拥有指定类型的Buff
	/// </summary>
	bool HasBuff(BuffCode buffCode);
}

