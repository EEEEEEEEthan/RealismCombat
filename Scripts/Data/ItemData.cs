using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
[Flags]
public enum EquipmentTypeCode : ulong
{
	None = 0,
	/// <summary>货币</summary>
	Money = 1UL << 0,
	/// <summary>头盔内衬</summary>
	HelmetLiner = 1UL << 1,
	/// <summary>头盔中层</summary>
	HelmetMidLayer = 1UL << 2,
	/// <summary>头盔</summary>
	Helmet = 1UL << 3,
	/// <summary>护手</summary>
	Gauntlet = 1UL << 4,
	/// <summary>胸甲内衬</summary>
	ChestLiner = 1UL << 5,
	/// <summary>胸甲中层</summary>
	ChestMidLayer = 1UL << 6,
	/// <summary>胸甲外套</summary>
	ChestOuter = 1UL << 7,
	/// <summary>武器</summary>
	Arm = 1UL << 8,
	/// <summary>刀</summary>
	Knife = 1UL << 10,
	/// <summary>剑</summary>
	Sword = 1UL << 11,
	/// <summary>锤</summary>
	Hammer = 1UL << 12,
	/// <summary>斧</summary>
	Axe = 1UL << 13,
	/// <summary>枪</summary>
	Spear = 1UL << 14,
	/// <summary>戟</summary>
	Halberd = 1UL << 15,
	/// <summary>盾</summary>
	Shield = 1UL << 16,
	/// <summary>腿甲内衬</summary>
	LegLiner = 1UL << 17,
	/// <summary>腿甲中层</summary>
	LegMidLayer = 1UL << 18,
	/// <summary>腿甲外层</summary>
	LegOuter = 1UL << 19,
}
public class SlotData
{
	public readonly EquipmentTypeCode allowedTypes;
	public ItemData? item;
	public SlotData(EquipmentTypeCode allowedTypes, ItemData? item = null)
	{
		this.allowedTypes = allowedTypes;
		this.item = item;
	}
	public SlotData(DataVersion version, BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			allowedTypes = (EquipmentTypeCode)reader.ReadUInt64();
			var hasItem = reader.ReadBoolean();
			if (hasItem) item = new(version: version, reader: reader);
		}
	}
	public bool CanPlace(ItemData? item)
	{
		if (item == null) return true;
		var itemType = ItemConfig.configs.TryGetValue(key: item.itemId, value: out var config) ? config.EquipmentType : EquipmentTypeCode.None;
		return (allowedTypes & itemType) != 0;
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write((ulong)allowedTypes);
			if (item is not null)
			{
				writer.Write(true);
				item.Serialize(writer);
			}
			else
			{
				writer.Write(false);
			}
		}
	}
}
public class ItemData : IItemContainer
{
	public readonly uint itemId;
	public readonly SlotData[] slots;
	public readonly double weight; // 单个物品的重量，单位：公斤
	public readonly double length; // 物品长度（米），如果是武器则为武器长度，否则为最长径
	public int count;
	public IReadOnlyList<ItemData?> items => slots.Select(s => s.item).ToList().AsReadOnly();
	public event Action? ItemsChanged;
	public ItemData(uint itemId, int count)
	{
		this.itemId = itemId;
		this.count = count;
		var config = ItemConfig.configs.GetValueOrDefault(itemId);
		weight = config?.Weight ?? 0.0; // 根据配置设置单个物品重量
		length = config?.Length ?? 0.0; // 根据配置设置长度
		var capacity = config?.SlotCapacity ?? 0;
		slots = new SlotData[capacity];
		for (var i = 0; i < capacity; i++)
		{
			var allowedTypes = config?.GetSlotAllowedTypes(i) ?? EquipmentTypeCode.None;
			slots[i] = new(allowedTypes: allowedTypes);
		}
	}
	public ItemData(DataVersion version, BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			itemId = reader.ReadUInt32();
			count = reader.ReadInt32();
			weight = reader.ReadDouble(); // 读取单个物品重量
			length = reader.ReadDouble(); // 读取长度
			var slotCount = reader.ReadInt32();
			var capacity = ItemConfig.configs.TryGetValue(key: itemId, value: out var config) ? config.SlotCapacity : 0;
			slots = new SlotData[capacity];
			for (var i = 0; i < slotCount && i < capacity; ++i) slots[i] = new(version: version, reader: reader);
			for (var i = slotCount; i < capacity; i++)
			{
				var allowedTypes =
					ItemConfig.configs.TryGetValue(key: itemId, value: out var config2) ? config2.GetSlotAllowedTypes(i) : EquipmentTypeCode.None;
				slots[i] = new(allowedTypes: allowedTypes);
			}
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write(itemId);
			writer.Write(count);
			writer.Write(weight); // 写入单个物品重量
			writer.Write(length); // 写入长度
			writer.Write(slots.Length);
			foreach (var slot in slots) slot.Serialize(writer);
		}
	}
	public void SetSlot(int index, ItemData? value)
	{
		if (index < 0 || index >= slots.Length) throw new ArgumentOutOfRangeException(nameof(index));
		if (!slots[index].CanPlace(value)) throw new InvalidOperationException($"物品类型 {value?.itemId} 不允许放入此槽位");
		slots[index].item = value;
		ItemsChanged?.Invoke();
	}
	public double GetTotalWeight() // 获取该数量物品的总重量
		=>
			weight * count;
	public override string ToString() =>
		$"{nameof(ItemData)}({nameof(itemId)}={itemId}, {nameof(count)}={count}, {nameof(weight)}={weight}kg, {nameof(length)}={length}m, {nameof(slots)}={slots.Length}, {nameof(GetTotalWeight)}={GetTotalWeight()}kg)";
}
public class ItemConfig
{
	public static readonly Dictionary<uint, ItemConfig> configs = new();
	static ItemConfig()
	{
		configs[0] = new(
			itemId: 0,
			name: "第纳尔",
			slotCapacity: 0,
			equipmentType: EquipmentTypeCode.Money,
			slotAllowedTypes: EquipmentTypeCode.None,
			slotAllowedTypesPerSlot: null,
			weight: 0.005,
			length: 0.01); // 金币很轻
		configs[1] = new(
			itemId: 1,
			name: "棉质内衬",
			slotCapacity: 3,
			equipmentType: EquipmentTypeCode.ChestLiner,
			slotAllowedTypes: EquipmentTypeCode.None,
			slotAllowedTypesPerSlot: [EquipmentTypeCode.ChestMidLayer, EquipmentTypeCode.Arm, EquipmentTypeCode.Arm,],
			weight: 1.5,
			length: 0.8); // 棉质衣物重量
		configs[2] = new(
			itemId: 2,
			name: "链甲",
			slotCapacity: 3,
			equipmentType: EquipmentTypeCode.ChestMidLayer,
			slotAllowedTypes: EquipmentTypeCode.None,
			slotAllowedTypesPerSlot: [EquipmentTypeCode.ChestOuter, EquipmentTypeCode.Arm, EquipmentTypeCode.Arm,],
			weight: 12.0,
			length: 0.9); // 链甲较重
		configs[3] = new(
			itemId: 3,
			name: "皮帽",
			slotCapacity: 0,
			equipmentType: EquipmentTypeCode.HelmetLiner,
			slotAllowedTypes: EquipmentTypeCode.None,
			slotAllowedTypesPerSlot: null,
			weight: 0.8,
			length: 0.3); // 皮帽
		configs[4] = new(
			itemId: 4,
			name: "布手套",
			slotCapacity: 0,
			equipmentType: EquipmentTypeCode.Gauntlet,
			slotAllowedTypes: EquipmentTypeCode.None,
			slotAllowedTypesPerSlot: null,
			weight: 0.2,
			length: 0.25); // 布手套
		configs[5] = new(
			itemId: 5,
			name: "短刀",
			slotCapacity: 0,
			equipmentType: EquipmentTypeCode.Arm | EquipmentTypeCode.Knife,
			slotAllowedTypes: EquipmentTypeCode.None,
			slotAllowedTypesPerSlot: null,
			weight: 1.2,
			length: 0.4); // 短刀
		configs[6] = new(
			itemId: 6,
			name: "长剑",
			slotCapacity: 0,
			equipmentType: EquipmentTypeCode.Arm | EquipmentTypeCode.Sword,
			slotAllowedTypes: EquipmentTypeCode.None,
			slotAllowedTypesPerSlot: null,
			weight: 3.0,
			length: 1.2); // 长剑
		configs[7] = new(
			itemId: 7,
			name: "短剑",
			slotCapacity: 0,
			equipmentType: EquipmentTypeCode.Arm | EquipmentTypeCode.Sword,
			slotAllowedTypes: EquipmentTypeCode.None,
			slotAllowedTypesPerSlot: null,
			weight: 1.5,
			length: 0.6); // 短剑更轻更短
		configs[8] = new(
			itemId: 8,
			name: "圆盾",
			slotCapacity: 0,
			equipmentType: EquipmentTypeCode.Shield,
			slotAllowedTypes: EquipmentTypeCode.None,
			slotAllowedTypesPerSlot: null,
			weight: 4.0,
			length: 0.8); // 圆盾
	}
	readonly IReadOnlyList<EquipmentTypeCode>? slotAllowedTypesPerSlot;
	public int SlotCapacity { get; }
	public EquipmentTypeCode EquipmentType { get; }
	public EquipmentTypeCode SlotAllowedTypes { get; }
	public double Weight { get; } // 物品重量（公斤）
	public double Length { get; } // 物品长度（米），如果是武器则为武器长度，否则为最长径
	public uint ItemId { get; private set; }
	public string Name { get; private set; }
	ItemConfig(
		uint itemId,
		string name,
		int slotCapacity,
		EquipmentTypeCode equipmentType,
		EquipmentTypeCode slotAllowedTypes,
		IReadOnlyList<EquipmentTypeCode>? slotAllowedTypesPerSlot,
		double weight,
		double length)
	{
		ItemId = itemId;
		Name = name;
		SlotCapacity = slotCapacity;
		EquipmentType = equipmentType;
		SlotAllowedTypes = slotAllowedTypes;
		this.slotAllowedTypesPerSlot = slotAllowedTypesPerSlot;
		Weight = weight;
		Length = length;
	}
	public EquipmentTypeCode GetSlotAllowedTypes(int slotIndex)
	{
		if (slotAllowedTypesPerSlot != null && slotIndex >= 0 && slotIndex < slotAllowedTypesPerSlot.Count) return slotAllowedTypesPerSlot[slotIndex];
		return SlotAllowedTypes;
	}
}
