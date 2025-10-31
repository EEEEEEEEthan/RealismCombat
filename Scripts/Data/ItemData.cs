using System;
using System.Collections.Generic;
using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;

[Flags]
public enum EquipmentType : ulong
{
	None = 0,
	钱 = 1UL << 0,
	头盔内衬 = 1UL << 1,
	头盔中层 = 1UL << 2,
	头盔 = 1UL << 3,
	护手 = 1UL << 4,
	胸甲内衬 = 1UL << 5,
	胸甲中层 = 1UL << 6,
	胸甲外套 = 1UL << 7,
	单手武器 = 1UL << 8,
	双手武器 = 1UL << 9,
	刀 = 1UL << 10,
	剑 = 1UL << 11,
	锤 = 1UL << 12,
	斧 = 1UL << 13,
	枪 = 1UL << 14,
	戟 = 1UL << 15,
	盾 = 1UL << 16,
}
public class ItemData : IItemContainer
{
	public readonly uint itemId;
	public int count;
	public readonly ItemData?[] slots;
	public IReadOnlyList<ItemData?> items => slots;
	public event Action? ItemsChanged;
	public ItemData(uint itemId, int count)
	{
		this.itemId = itemId;
		this.count = count;
		var capacity = ItemConfig.Configs.TryGetValue(itemId, out var config) ? config.slotCapacity : 0;
		slots = new ItemData?[capacity];
	}
	public ItemData(DataVersion version, BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			itemId = reader.ReadUInt32();
			count = reader.ReadInt32();
			var slotCount = reader.ReadInt32();
			var capacity = ItemConfig.Configs.TryGetValue(itemId, out var config) ? config.slotCapacity : 0;
			slots = new ItemData?[capacity];
			for (var i = 0; i < slotCount && i < capacity; ++i)
			{
				var hasItem = reader.ReadBoolean();
				if (hasItem)
				{
					slots[i] = new(version: version, reader: reader);
				}
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
			foreach (var slot in slots)
			{
				if (slot is not null)
				{
					writer.Write(true);
					slot.Serialize(writer);
				}
				else
				{
					writer.Write(false);
				}
			}
		}
	}
	public void SetSlot(int index, ItemData? value)
	{
		if (index < 0 || index >= slots.Length) throw new ArgumentOutOfRangeException(nameof(index));
		slots[index] = value;
		ItemsChanged?.Invoke();
	}
	public override string ToString() => $"{nameof(ItemData)}({nameof(itemId)}={itemId}, {nameof(count)}={count}, {nameof(slots)}={slots.Length})";
}
public class ItemConfig
{
	public static readonly Dictionary<uint, ItemConfig> Configs = new();
	public uint itemId { get; private set; }
	public string name { get; private set; }
	public int slotCapacity { get; private set; }
	public EquipmentType equipmentType { get; private set; }
	private ItemConfig(uint itemId, string name, int slotCapacity = 0, EquipmentType equipmentType = EquipmentType.None)
	{
		this.itemId = itemId;
		this.name = name;
		this.slotCapacity = slotCapacity;
		this.equipmentType = equipmentType;
	}
	static ItemConfig()
	{
		Configs[0] = new ItemConfig(0, "第纳尔", 0, EquipmentType.钱);
		Configs[1] = new ItemConfig(1, "棉质内衬", 0, EquipmentType.胸甲内衬);
		Configs[2] = new ItemConfig(2, "粗布绵甲", 0, EquipmentType.胸甲外套);
		Configs[3] = new ItemConfig(3, "皮帽", 0, EquipmentType.头盔);
		Configs[4] = new ItemConfig(4, "布手套", 0, EquipmentType.护手);
		Configs[5] = new ItemConfig(5, "短刀", 0, EquipmentType.单手武器 | EquipmentType.刀);
		Configs[6] = new ItemConfig(6, "长剑", 0, EquipmentType.单手武器 | EquipmentType.剑);
		Configs[7] = new ItemConfig(7, "双手剑", 0, EquipmentType.双手武器 | EquipmentType.剑);
		Configs[8] = new ItemConfig(8, "圆盾", 0, EquipmentType.盾);
	}
}
