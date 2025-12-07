using System.Collections.Generic;
/// <summary>
///     Buff拥有者接口
/// </summary>
public interface IBuffOwner
{
	/// <summary>
	///     获取所有Buff列表
	/// </summary>
	List<Buff> Buffs { get; }
}
