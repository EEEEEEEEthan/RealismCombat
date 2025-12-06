using System;
using System.Collections.Generic;
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
/// <summary>
///     装备类型显示名称工具
/// </summary>
public static class ItemFlagCodeExtensions
{
	/// <summary>
	///     获取装备类型的显示名称
	/// </summary>
	public static string GetDisplayName(this ItemFlagCode flag)
	{
		if (flag == 0) return "未定义";
		var names = new List<string>();
		foreach (ItemFlagCode code in Enum.GetValues(typeof(ItemFlagCode)))
		{
			if (code == 0) continue;
			if (flag.HasFlag(code))
				names.Add(code switch
				{
					ItemFlagCode.Arm => "武器",
					ItemFlagCode.InnerLayer => "内衬",
					ItemFlagCode.MiddleLayer => "中层",
					ItemFlagCode.OuterCoat => "外套",
					ItemFlagCode.Belt => "皮带",
					_ => code.ToString(),
				});
		}
		return string.Join("、", names);
	}
}
