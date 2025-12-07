using System;
using System.Collections.Generic;
using System.IO;
/// <summary>
/// 物品标识枚举,用于索引配置与资源
/// </summary>
public enum ItemIdCode
{
	/// <summary>
	/// 长剑
	/// </summary>
	LongSword,
	/// <summary>
	/// 绵甲上衣
	/// </summary>
	CottonLiner,
	/// <summary>
	/// 链甲上衣
	/// </summary>
	ChainMail,
	/// <summary>
	/// 板甲上衣
	/// </summary>
	PlateArmor,
	/// <summary>
	/// 装备多把武器的皮带
	/// </summary>
	Belt,
	/// <summary>
	/// 棉质护腿
	/// </summary>
	CottonPants,
	/// <summary>
	/// 链甲护腿
	/// </summary>
	ChainChausses,
	/// <summary>
	/// 板甲护腿
	/// </summary>
	PlateGreaves,
}
/// <summary>
///     战斗中可以被选择的装备实体
/// </summary>
public class Item : ICombatTarget, IItemContainer, IBuffOwner
{
	public readonly struct ItemConfig
	{
		public string Name { get; init; }
		public string? Description { get; init; }
		public ItemFlagCode Flag { get; init; }
		public ItemFlagCode[]? SlotFlags { get; init; }
		public double Length { get; init; }
		public double Weight { get; init; }
		public int HitPointMax { get; init; }
		public DamageProfile DamageProfile { get; init; }
		public Protection Protection { get; init; }
	}
	static readonly Dictionary<ItemIdCode, ItemConfig> configs = new()
	{
		{
			ItemIdCode.LongSword,
			new()
			{
				Name = "长剑",
				Description = "比短剑长一点",
				Flag = ItemFlagCode.Arm,
				SlotFlags = [],
				Length = 100.0,
				Weight = 1.2,
				HitPointMax = 10,
				DamageProfile = new(
					new Damage(4f, 0f, 3f),
					new Damage(0f, 4f, 0f),
					Damage.Zero
				),
				Protection = Protection.Zero,
			}
		},
		{
			ItemIdCode.CottonLiner,
			new()
			{
				Name = "绵甲",
				Description = "廉价的武装衣.征召兵们最常穿的装备",
				Flag = ItemFlagCode.TorsoArmor,
				SlotFlags = [],
				Length = 60.0,
				Weight = 0.8,
				HitPointMax = 12,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new Protection(1f, 1f, 1f),
			}
		},
		{
			ItemIdCode.ChainMail,
			new()
			{
				Name = "链甲",
				Description = "由铁环串联而成的护甲,他们柔软但是沉重.通常需要耗费一个工匠数年时间才能完成",
				Flag = ItemFlagCode.TorsoArmor,
				SlotFlags = [],
				Length = 65.0,
				Weight = 6.5,
				HitPointMax = 18,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new Protection(4f, 2f, 1f),
			}
		},
		{
			ItemIdCode.PlateArmor,
			new()
			{
				Name = "板甲",
				Description = "由金属板制成的护甲,防护能力强大,但是重量也很惊人",
				Flag = ItemFlagCode.TorsoArmor,
				SlotFlags = [],
				Length = 70.0,
				Weight = 12.0,
				HitPointMax = 25,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new Protection(4f, 4f, 2f),
			}
		},
		{
			ItemIdCode.Belt,
			new()
			{
				Name = "皮带",
				Description = "用来固定武器的皮带.上面可以挂很多武器",
				Flag = ItemFlagCode.Belt,
				SlotFlags = [ItemFlagCode.Arm, ItemFlagCode.Arm, ItemFlagCode.Arm, ItemFlagCode.Arm,],
				Length = 90.0,
				Weight = 0.5,
				HitPointMax = 8,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = Protection.Zero,
			}
		},
		{
			ItemIdCode.CottonPants,
			new()
			{
				Name = "棉裤",
				Description = "填充棉絮的护腿,保暖又廉价",
				Flag = ItemFlagCode.LegArmor,
				SlotFlags = [],
				Length = 90.0,
				Weight = 0.6,
				HitPointMax = 8,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new Protection(1f, 1f, 1f),
			}
		},
		{
			ItemIdCode.ChainChausses,
			new()
			{
				Name = "链甲护腿",
				Description = "由细密铁环编织的护腿,沉重但可靠",
				Flag = ItemFlagCode.LegArmor,
				SlotFlags = [],
				Length = 95.0,
				Weight = 4.5,
				HitPointMax = 14,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new Protection(4f, 2f, 1f),
			}
		},
		{
			ItemIdCode.PlateGreaves,
			new()
			{
				Name = "板甲护腿",
				Description = "覆盖小腿的金属板,能挡住大部分攻击",
				Flag = ItemFlagCode.LegArmor,
				SlotFlags = [],
				Length = 95.0,
				Weight = 7.5,
				HitPointMax = 20,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new Protection(4f, 4f, 2f),
			}
		},
	};
	public static IReadOnlyDictionary<ItemIdCode, ItemConfig> Configs => configs;
	public static Item Create(ItemIdCode id)
	{
		if (!Configs.TryGetValue(id, out var config)) throw new NotSupportedException($"unexpected id: {id}");
		return new(id, config);
	}
	static ItemSlot[] CreateSlots(ItemFlagCode[]? slotFlags, IItemContainer owner)
	{
		if (slotFlags == null || slotFlags.Length == 0) return [];
		var slots = new ItemSlot[slotFlags.Length];
		for (var i = 0; i < slotFlags.Length; i++) slots[i] = new(slotFlags[i], owner);
		return slots;
	}
	public readonly ItemFlagCode flag;
	public readonly ItemIdCode id;
	readonly List<Buff> buffs = [];
	public string Name { get; }
	/// <summary>
	///     装备描述
	/// </summary>
	public string Description { get; }
	public ItemSlot[] Slots { get; }
	public PropertyInt HitPoint { get; }
	public double Length { get; }
	public double Weight { get; }
	public DamageProfile DamageProfile { get; }
	public Protection Protection { get; }
	public bool Available => HitPoint.value > 0;
	public IReadOnlyList<Buff> Buffs => buffs;
	Item(ItemIdCode id, ItemConfig config)
	{
		this.id = id;
		flag = config.Flag;
		Name = config.Name;
		Description = string.IsNullOrEmpty(config.Description) ? config.Name : config.Description;
		Length = config.Length;
		Weight = config.Weight;
		Slots = CreateSlots(config.SlotFlags, this);
		HitPoint = new(config.HitPointMax, config.HitPointMax);
		DamageProfile = config.DamageProfile;
		Protection = config.Protection;
	}
	public void AddBuff(Buff buff) => buffs.Add(buff);
	public void RemoveBuff(Buff buff) => buffs.Remove(buff);
	public bool HasBuff(BuffCode buff)
	{
		foreach (var b in buffs)
			if (b.code == buff)
				return true;
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
			using (reader.ReadScope())
			{
				reader.ReadUInt64();
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
}
