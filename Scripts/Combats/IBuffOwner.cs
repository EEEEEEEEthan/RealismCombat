using System.Collections.Generic;
/// <summary>
///     Buff拥有者接口
/// </summary>
public interface IBuffOwner
{
	/// <summary>
	///     获取所有Buff字典，键为BuffCode
	/// </summary>
	Dictionary<BuffCode, Buff> Buffs { get; }
}
