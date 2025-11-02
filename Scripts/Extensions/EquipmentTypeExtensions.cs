using System.Collections.Generic;
using RealismCombat.Data;
namespace RealismCombat.Extensions;
static class EquipmentTypeExtensions
{
	public static string GetName(this EquipmentTypeCode type)
	{
		if (type == EquipmentTypeCode.None) return "无";
		var parts = new List<string>();
		if ((type & EquipmentTypeCode.Money) != 0) parts.Add("货币");
		if ((type & EquipmentTypeCode.HelmetLiner) != 0) parts.Add("头盔内衬");
		if ((type & EquipmentTypeCode.HelmetMidLayer) != 0) parts.Add("头盔中层");
		if ((type & EquipmentTypeCode.Helmet) != 0) parts.Add("头盔");
		if ((type & EquipmentTypeCode.Gauntlet) != 0) parts.Add("护手");
		if ((type & EquipmentTypeCode.ChestLiner) != 0) parts.Add("胸甲内衬");
		if ((type & EquipmentTypeCode.ChestMidLayer) != 0) parts.Add("胸甲中层");
		if ((type & EquipmentTypeCode.ChestOuter) != 0) parts.Add("胸甲外套");
		if ((type & EquipmentTypeCode.Arm) != 0) parts.Add("武器");
		if ((type & EquipmentTypeCode.Knife) != 0) parts.Add("刀");
		if ((type & EquipmentTypeCode.Sword) != 0) parts.Add("剑");
		if ((type & EquipmentTypeCode.Hammer) != 0) parts.Add("锤");
		if ((type & EquipmentTypeCode.Axe) != 0) parts.Add("斧");
		if ((type & EquipmentTypeCode.Spear) != 0) parts.Add("枪");
		if ((type & EquipmentTypeCode.Halberd) != 0) parts.Add("戟");
		if ((type & EquipmentTypeCode.Shield) != 0) parts.Add("盾");
		if ((type & EquipmentTypeCode.LegLiner) != 0) parts.Add("腿甲内衬");
		if ((type & EquipmentTypeCode.LegMidLayer) != 0) parts.Add("腿甲中层");
		if ((type & EquipmentTypeCode.LegOuter) != 0) parts.Add("腿甲外层");
		return parts.Count == 0 ? "未知类型" : string.Join(separator: "或", values: parts);
	}
	public static string GetShortName(this EquipmentTypeCode type)
	{
		if (type == EquipmentTypeCode.None) return "无";
		var parts = new List<string>();
		if ((type & EquipmentTypeCode.Money) != 0) parts.Add("货币");
		if ((type & EquipmentTypeCode.HelmetLiner) != 0) parts.Add("内衬");
		if ((type & EquipmentTypeCode.HelmetMidLayer) != 0) parts.Add("中层");
		if ((type & EquipmentTypeCode.Helmet) != 0) parts.Add("头盔");
		if ((type & EquipmentTypeCode.Gauntlet) != 0) parts.Add("护手");
		if ((type & EquipmentTypeCode.ChestLiner) != 0) parts.Add("内衬");
		if ((type & EquipmentTypeCode.ChestMidLayer) != 0) parts.Add("中层");
		if ((type & EquipmentTypeCode.ChestOuter) != 0) parts.Add("外套");
		if ((type & EquipmentTypeCode.Arm) != 0) parts.Add("武器");
		if ((type & EquipmentTypeCode.Knife) != 0) parts.Add("刀");
		if ((type & EquipmentTypeCode.Sword) != 0) parts.Add("剑");
		if ((type & EquipmentTypeCode.Hammer) != 0) parts.Add("锤");
		if ((type & EquipmentTypeCode.Axe) != 0) parts.Add("斧");
		if ((type & EquipmentTypeCode.Spear) != 0) parts.Add("枪");
		if ((type & EquipmentTypeCode.Halberd) != 0) parts.Add("戟");
		if ((type & EquipmentTypeCode.Shield) != 0) parts.Add("盾");
		if ((type & EquipmentTypeCode.LegLiner) != 0) parts.Add("内衬");
		if ((type & EquipmentTypeCode.LegMidLayer) != 0) parts.Add("中层");
		if ((type & EquipmentTypeCode.LegOuter) != 0) parts.Add("外层");
		return parts.Count == 0 ? "未知类型" : string.Join(separator: "或", values: parts);
	}
}
