using System;
/// <summary>
///     装备类型掩码
/// </summary>
[Flags]
public enum ItemFlagCode
{
	/// <summary>
	///     武器类型装备
	/// </summary>
	Arm = 1 << 0,
}
