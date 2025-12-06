using System;
using System.Collections.Generic;
using System.IO;
public enum ItemIdCode
{
	LongSword,
	CottonLiner,
	ChainMail,
	PlateArmor,
	Belt,
}
/// <summary>
///     战斗中可以被选择的装备实体
/// </summary>
public class Item : ICombatTarget, IItemContainer, IBuffOwner
{
	public readonly ItemFlagCode flag;
	public readonly ItemIdCode id;
	readonly List<Buff> buffs = [];
	public string Name { get; }
	public ItemSlot[] Slots { get; }
	public PropertyInt HitPoint { get; }
	public double Length { get; }
	public double Weight { get; }
	public bool Available => HitPoint.value > 0;
	public IReadOnlyList<Buff> Buffs => buffs;
	Item(ItemIdCode id, ItemConfig config)
	{
		this.id = id;
		flag = config.Flag;
		Name = config.Name;
		Length = config.Length;
		Weight = config.Weight;
		Slots = CreateSlots(config.SlotFlags, this);
		HitPoint = new(config.HitPointMax, config.HitPointMax);
	}
	static ItemSlot[] CreateSlots(ItemFlagCode[]? slotFlags, IItemContainer owner)
	{
		if (slotFlags == null || slotFlags.Length == 0) return Array.Empty<ItemSlot>();
		var slots = new ItemSlot[slotFlags.Length];
		for (var i = 0; i < slotFlags.Length; i++) slots[i] = new(slotFlags[i], owner);
		return slots;
	}
	public static Item Create(ItemIdCode id)
	{
		if (!Configs.TryGetValue(id, out var config)) throw new NotSupportedException($"unexpected id: {id}");
		return new(id, config);
	}
	public void AddBuff(Buff buff)
	{
		buffs.Add(buff);
	}
	public void RemoveBuff(Buff buff)
	{
		buffs.Remove(buff);
	}
	public bool HasBuff(BuffCode buffCode)
	{
		foreach (var buff in buffs)
		{
			if (buff.code == buffCode)
			{
				return true;
			}
		}
		return false;
	}
	#region serialize
	public static Item Load(BinaryReader reader)
	{
		using var _ = reader.ReadScope();
		var id = (ItemIdCode)reader.ReadUInt64();
		var item = Create(id);
		var slotCount = reader.ReadInt32();
		var trueSlotCount = Math.Min(slotCount, item.Slots.Length);
		for (var i = 0; i < trueSlotCount; i++) item.Slots[i].Deserialize(reader);
		for (var i = trueSlotCount; i < slotCount; i++) new ItemSlot(default, item).Deserialize(reader);
		var buffCount = reader.ReadInt32();
		for (var i = 0; i < buffCount; i++)
		{
			using (reader.ReadScope())
			{
				reader.ReadUInt64();
			}
		}
		return item;
	}
	public void Serialize(BinaryWriter writer)
	{
		using var _ = writer.WriteScope();
		writer.Write((ulong)id);
		writer.Write(Slots.Length);
		for (var i = 0; i < Slots.Length; i++) Slots[i].Serialize(writer);
		writer.Write(0);
	}
	#endregion
	public readonly struct ItemConfig
	{
		public string Name { get; init; }
		public ItemFlagCode Flag { get; init; }
		public ItemFlagCode[]? SlotFlags { get; init; }
		public double Length { get; init; }
		public double Weight { get; init; }
		public int HitPointMax { get; init; }
	}
	static readonly Dictionary<ItemIdCode, ItemConfig> configs = new()
	{
		{
			ItemIdCode.LongSword,
			new ItemConfig
			{
				Name = "长剑",
				Flag = ItemFlagCode.Arm,
				SlotFlags = Array.Empty<ItemFlagCode>(),
				Length = 100.0,
				Weight = 1.2,
				HitPointMax = 10,
			}
		},
		{
			ItemIdCode.CottonLiner,
			new ItemConfig
			{
				Name = "棉质内衬",
				Flag = ItemFlagCode.InnerLayer,
				SlotFlags = new[] { ItemFlagCode.MiddleLayer, ItemFlagCode.Belt, },
				Length = 60.0,
				Weight = 0.8,
				HitPointMax = 12,
			}
		},
		{
			ItemIdCode.ChainMail,
			new ItemConfig
			{
				Name = "链甲",
				Flag = ItemFlagCode.MiddleLayer,
				SlotFlags = new[] { ItemFlagCode.OuterCoat, ItemFlagCode.Belt, },
				Length = 65.0,
				Weight = 6.5,
				HitPointMax = 18,
			}
		},
		{
			ItemIdCode.PlateArmor,
			new ItemConfig
			{
				Name = "板甲",
				Flag = ItemFlagCode.OuterCoat,
				SlotFlags = new[] { ItemFlagCode.Belt, },
				Length = 70.0,
				Weight = 12.0,
				HitPointMax = 25,
			}
		},
		{
			ItemIdCode.Belt,
			new ItemConfig
			{
				Name = "皮带",
				Flag = ItemFlagCode.Belt,
				SlotFlags = new[] { ItemFlagCode.Arm, ItemFlagCode.Arm, ItemFlagCode.Arm, ItemFlagCode.Arm, },
				Length = 90.0,
				Weight = 0.5,
				HitPointMax = 8,
			}
		},
	};
	public static IReadOnlyDictionary<ItemIdCode, ItemConfig> Configs => configs;
}

