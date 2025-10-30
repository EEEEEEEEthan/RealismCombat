using System.Collections.Generic;
using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
public class ItemData
{
	public readonly uint itemId;
	public int count;
	public readonly ItemData?[] slots;
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
	public override string ToString() => $"{nameof(ItemData)}({nameof(itemId)}={itemId}, {nameof(count)}={count}, {nameof(slots)}={slots.Length})";
}
public class ItemConfig
{
	public static readonly Dictionary<uint, ItemConfig> Configs = new();
	public uint itemId { get; private set; }
	public string name { get; private set; }
	public int slotCapacity { get; private set; }
	private ItemConfig(uint itemId, string name, int slotCapacity = 0)
	{
		this.itemId = itemId;
		this.name = name;
		this.slotCapacity = slotCapacity;
	}
	static ItemConfig()
	{
		Configs[0] = new ItemConfig(0, "第纳尔", 0);
	}
}
