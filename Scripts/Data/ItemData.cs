using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
[Flags]
public enum EquipmentType : ulong
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
	public EquipmentType allowedTypes;
	public ItemData? item;
	public SlotData(EquipmentType allowedTypes, ItemData? item = null)
	{
		this.allowedTypes = allowedTypes;
		this.item = item;
	}
	public SlotData(DataVersion version, BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			allowedTypes = (EquipmentType)reader.ReadUInt64();
			var hasItem = reader.ReadBoolean();
			if (hasItem) item = new(version: version, reader: reader);
		}
	}
	public bool CanPlace(ItemData? item)
	{
		if (item == null) return true;
		var itemType = ItemConfig.Configs.TryGetValue(key: item.itemId, value: out var config) ? config.equipmentType : EquipmentType.None;
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
	public int count;
	public IReadOnlyList<ItemData?> items => slots.Select(s => s.item).ToList().AsReadOnly();
	public event Action? ItemsChanged;
	public ItemData(uint itemId, int count)
	{
		this.itemId = itemId;
		this.count = count;
		var capacity = ItemConfig.Configs.TryGetValue(key: itemId, value: out var config) ? config.slotCapacity : 0;
		slots = new SlotData[capacity];
		for (var i = 0; i < capacity; i++)
		{
			var allowedTypes = ItemConfig.Configs.TryGetValue(key: itemId, value: out var config2) ? config2.GetSlotAllowedTypes(i) : EquipmentType.None;
			slots[i] = new(allowedTypes: allowedTypes);
		}
	}
	public ItemData(DataVersion version, BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			itemId = reader.ReadUInt32();
			count = reader.ReadInt32();
			var slotCount = reader.ReadInt32();
			var capacity = ItemConfig.Configs.TryGetValue(key: itemId, value: out var config) ? config.slotCapacity : 0;
			slots = new SlotData[capacity];
			for (var i = 0; i < slotCount && i < capacity; ++i) slots[i] = new(version: version, reader: reader);
			for (var i = slotCount; i < capacity; i++)
			{
				var allowedTypes = ItemConfig.Configs.TryGetValue(key: itemId, value: out var config2) ? config2.GetSlotAllowedTypes(i) : EquipmentType.None;
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
	public override string ToString() => $"{nameof(ItemData)}({nameof(itemId)}={itemId}, {nameof(count)}={count}, {nameof(slots)}={slots.Length})";
}
public class ItemConfig
{
	public static readonly Dictionary<uint, ItemConfig> Configs = new();
	static ItemConfig()
	{
		Configs[0] = new(itemId: 0, name: "第纳尔", slotCapacity: 0, equipmentType: EquipmentType.Money);
		Configs[1] = new(itemId: 1,
			name: "棉质内衬",
			slotCapacity: 3,
			equipmentType: EquipmentType.ChestLiner,
			slotAllowedTypesPerSlot: new[]
			{
				EquipmentType.ChestMidLayer, EquipmentType.Arm,
				EquipmentType.Arm,
			});
		Configs[2] = new(itemId: 2,
			name: "链甲",
			slotCapacity: 3,
			equipmentType: EquipmentType.ChestMidLayer,
			slotAllowedTypesPerSlot: new[]
			{
				EquipmentType.ChestOuter, EquipmentType.Arm,
				EquipmentType.Arm,
			});
		Configs[3] = new(itemId: 3, name: "皮帽", slotCapacity: 0, equipmentType: EquipmentType.HelmetLiner);
		Configs[4] = new(itemId: 4, name: "布手套", slotCapacity: 0, equipmentType: EquipmentType.Gauntlet);
		Configs[5] = new(itemId: 5, name: "短刀", slotCapacity: 0, equipmentType: EquipmentType.Arm | EquipmentType.Knife);
		Configs[6] = new(itemId: 6, name: "长剑", slotCapacity: 0, equipmentType: EquipmentType.Arm | EquipmentType.Sword);
		Configs[7] = new(itemId: 7, name: "双手剑", slotCapacity: 0, equipmentType: EquipmentType.Arm | EquipmentType.Sword);
		Configs[8] = new(itemId: 8, name: "圆盾", slotCapacity: 0, equipmentType: EquipmentType.Shield);
	}
	readonly IReadOnlyList<EquipmentType>? slotAllowedTypesPerSlot;
	public int slotCapacity { get; }
	public EquipmentType equipmentType { get; }
	public EquipmentType slotAllowedTypes { get; }
	public uint itemId { get; private set; }
	public string name { get; private set; }
	ItemConfig(
		uint itemId,
		string name,
		int slotCapacity = 0,
		EquipmentType equipmentType = EquipmentType.None,
		EquipmentType slotAllowedTypes = EquipmentType.None,
		IReadOnlyList<EquipmentType>? slotAllowedTypesPerSlot = null)
	{
		this.itemId = itemId;
		this.name = name;
		this.slotCapacity = slotCapacity;
		this.equipmentType = equipmentType;
		this.slotAllowedTypes = slotAllowedTypes;
		this.slotAllowedTypesPerSlot = slotAllowedTypesPerSlot;
	}
	public EquipmentType GetSlotAllowedTypes(int slotIndex)
	{
		if (slotAllowedTypesPerSlot != null && slotIndex >= 0 && slotIndex < slotAllowedTypesPerSlot.Count) return slotAllowedTypesPerSlot[slotIndex];
		return slotAllowedTypes;
	}
}
