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
	///     躯干护甲
	/// </summary>
	TorsoArmor = 1 << 1,
	/// <summary>
	///     护手
	/// </summary>
	HandArmor = 1 << 2,
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
					ItemFlagCode.TorsoArmor => "躯干护甲",
					ItemFlagCode.HandArmor => "护手",
					ItemFlagCode.Belt => "皮带",
					_ => code.ToString(),
				});
		}
		return string.Join("、", names);
	}
}
