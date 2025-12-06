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
	/// <summary>
	///     内衬类型装备
	/// </summary>
	InnerLayer = 1 << 1,
	/// <summary>
	///     中层类型装备
	/// </summary>
	MiddleLayer = 1 << 2,
	/// <summary>
	///     外套类型装备
	/// </summary>
	OuterCoat = 1 << 3,
	/// <summary>
	///     皮带类型装备
	/// </summary>
	Belt = 1 << 4,
}
