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
	///     腿部护甲
	/// </summary>
	LegArmor = 1 << 3,
	/// <summary>
	///     皮带类型装备
	/// </summary>
	Belt = 1 << 4,
	/// <summary>
	///     躯干中层护甲
	/// </summary>
	TorsoArmorMiddle = 1 << 5,
	/// <summary>
	///     躯干外层护甲
	/// </summary>
	TorsoArmorOuter = 1 << 6,
	/// <summary>
	///     裆部中层护甲
	/// </summary>
	LegArmorMiddle = 1 << 7,
	/// <summary>
	///     裆部外层护甲
	/// </summary>
	LegArmorOuter = 1 << 8,
	/// <summary>
	///     头部内层护甲
	/// </summary>
	HeadArmor = 1 << 9,
	/// <summary>
	///     头部中层护甲
	/// </summary>
	HeadArmorMiddle = 1 << 10,
	/// <summary>
	///     头部外层护甲
	/// </summary>
	HeadArmorOuter = 1 << 11,
	Armor = TorsoArmor | TorsoArmorMiddle | TorsoArmorOuter | HandArmor | LegArmor | LegArmorMiddle | LegArmorOuter | HeadArmor | HeadArmorMiddle | HeadArmorOuter,
}
/// <summary>
///     装备类型显示名称工具
/// </summary>
public static class ItemFlagCodeExtensions
{
	/// <summary>
	///     获取装备类型的显示名称
	/// </summary>
	public static string DisplayName(this ItemFlagCode flag)
	{
		if (flag == 0) return "未定义";
		var names = new List<string>();
		foreach (ItemFlagCode code in Enum.GetValues(typeof(ItemFlagCode)))
		{
			if (code == 0 || code == ItemFlagCode.Armor) continue;
			if (flag.HasFlag(code))
				names.Add(code switch
				{
					ItemFlagCode.Arm => "武器",
					ItemFlagCode.TorsoArmor => "内层上衣",
					ItemFlagCode.TorsoArmorMiddle => "中层上衣",
					ItemFlagCode.TorsoArmorOuter => "外层上衣",
					ItemFlagCode.HandArmor => "护手",
					ItemFlagCode.LegArmor => "内层腿甲",
					ItemFlagCode.LegArmorMiddle => "中层腿甲",
					ItemFlagCode.LegArmorOuter => "外层腿甲",
					ItemFlagCode.HeadArmor => "内层头盔",
					ItemFlagCode.HeadArmorMiddle => "中层头盔",
					ItemFlagCode.HeadArmorOuter => "外层头盔",
					ItemFlagCode.Belt => "皮带",
					_ => code.ToString(),
				});
		}
		return string.Join("、", names);
	}
}
