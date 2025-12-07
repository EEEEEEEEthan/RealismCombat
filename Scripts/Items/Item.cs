using System;
using System.Collections.Generic;
using System.IO;
/// <summary>
///     战斗中可以被选择的装备实体
/// </summary>
public class Item : ICombatTarget, IItemContainer, IBuffOwner
{
	public static Item Create(ItemIdCode id)
	{
		if (!ItemConfig.configs.TryGetValue(id, out var config)) throw new NotSupportedException($"unexpected id: {id}");
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
	public string Icon { get; }
	public string IconTag => $"[img=8x8]{Icon}[/img]";
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
		Icon = config.Icon;
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
