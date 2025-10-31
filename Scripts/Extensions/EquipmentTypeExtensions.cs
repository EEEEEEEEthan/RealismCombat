using System.Collections.Generic;
using RealismCombat.Data;
namespace RealismCombat.Extensions;
static class EquipmentTypeExtensions
{
	public static string GetName(this EquipmentType type)
	{
		if (type == EquipmentType.None) return "无";
		var parts = new List<string>();
		if ((type & EquipmentType.Money) != 0) parts.Add("货币");
		if ((type & EquipmentType.HelmetLiner) != 0) parts.Add("头盔内衬");
		if ((type & EquipmentType.HelmetMidLayer) != 0) parts.Add("头盔中层");
		if ((type & EquipmentType.Helmet) != 0) parts.Add("头盔");
		if ((type & EquipmentType.Gauntlet) != 0) parts.Add("护手");
		if ((type & EquipmentType.ChestLiner) != 0) parts.Add("胸甲内衬");
		if ((type & EquipmentType.ChestMidLayer) != 0) parts.Add("胸甲中层");
		if ((type & EquipmentType.ChestOuter) != 0) parts.Add("胸甲外套");
		if ((type & EquipmentType.OneHandedWeapon) != 0) parts.Add("单手武器");
		if ((type & EquipmentType.Knife) != 0) parts.Add("刀");
		if ((type & EquipmentType.Sword) != 0) parts.Add("剑");
		if ((type & EquipmentType.Hammer) != 0) parts.Add("锤");
		if ((type & EquipmentType.Axe) != 0) parts.Add("斧");
		if ((type & EquipmentType.Spear) != 0) parts.Add("枪");
		if ((type & EquipmentType.Halberd) != 0) parts.Add("戟");
		if ((type & EquipmentType.Shield) != 0) parts.Add("盾");
		if ((type & EquipmentType.LegLiner) != 0) parts.Add("腿甲内衬");
		if ((type & EquipmentType.LegMidLayer) != 0) parts.Add("腿甲中层");
		if ((type & EquipmentType.LegOuter) != 0) parts.Add("腿甲外层");
		return parts.Count == 0 ? "未知类型" : string.Join(separator: "或", values: parts);
	}
	public static string GetShortName(this EquipmentType type)
	{
		if (type == EquipmentType.None) return "无";
		var parts = new List<string>();
		if ((type & EquipmentType.Money) != 0) parts.Add("货币");
		if ((type & EquipmentType.HelmetLiner) != 0) parts.Add("内衬");
		if ((type & EquipmentType.HelmetMidLayer) != 0) parts.Add("中层");
		if ((type & EquipmentType.Helmet) != 0) parts.Add("头盔");
		if ((type & EquipmentType.Gauntlet) != 0) parts.Add("护手");
		if ((type & EquipmentType.ChestLiner) != 0) parts.Add("内衬");
		if ((type & EquipmentType.ChestMidLayer) != 0) parts.Add("中层");
		if ((type & EquipmentType.ChestOuter) != 0) parts.Add("外套");
		if ((type & EquipmentType.OneHandedWeapon) != 0) parts.Add("单手武器");
		if ((type & EquipmentType.Knife) != 0) parts.Add("刀");
		if ((type & EquipmentType.Sword) != 0) parts.Add("剑");
		if ((type & EquipmentType.Hammer) != 0) parts.Add("锤");
		if ((type & EquipmentType.Axe) != 0) parts.Add("斧");
		if ((type & EquipmentType.Spear) != 0) parts.Add("枪");
		if ((type & EquipmentType.Halberd) != 0) parts.Add("戟");
		if ((type & EquipmentType.Shield) != 0) parts.Add("盾");
		if ((type & EquipmentType.LegLiner) != 0) parts.Add("内衬");
		if ((type & EquipmentType.LegMidLayer) != 0) parts.Add("中层");
		if ((type & EquipmentType.LegOuter) != 0) parts.Add("外层");
		return parts.Count == 0 ? "未知类型" : string.Join(separator: "或", values: parts);
	}
}
