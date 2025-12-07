using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
/// <summary>
///     战斗中可以被选择的装备实体
/// </summary>
public class Item : ICombatTarget, IItemContainer
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
	static bool IsDamageProfileEmpty(DamageProfile profile) => profile.Swing.IsZero && profile.Thrust.IsZero && profile.Special.IsZero;
	static bool IsProtectionZero(Protection protection) => protection.slash <= 0f && protection.pierce <= 0f && protection.blunt <= 0f;
	static string FormatNumber(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
	static string FormatNumber(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
	static string FormatDamage(Damage damage) => $"{FormatNumber(damage.slash)}砍{FormatNumber(damage.pierce)}刺{FormatNumber(damage.blunt)}钝";
	static string FormatProtection(Protection protection) =>
		$"{FormatNumber(protection.slash)}砍{FormatNumber(protection.pierce)}刺{FormatNumber(protection.blunt)}钝";
	static string BuildDescription(ItemConfig config)
	{
		var lines = new List<string>();
		var hasDamageProfile = !IsDamageProfileEmpty(config.DamageProfile);
		if (hasDamageProfile)
		{
			lines.Add($"挥舞:{FormatDamage(config.DamageProfile.Swing)}");
			lines.Add($"捅扎:{FormatDamage(config.DamageProfile.Thrust)}");
			lines.Add($"特殊:{FormatDamage(config.DamageProfile.Special)}");
		}
		if (!IsProtectionZero(config.Protection)) lines.Add($"防护:{FormatProtection(config.Protection)}");
		lines.Add($"长度:{FormatNumber(config.Length)} 重量:{FormatNumber(config.Weight)} 耐久:{config.HitPointMax}");
		var story = config.Story;
		if (!string.IsNullOrWhiteSpace(story))
		{
			if (lines.Count > 0) lines.Add(string.Empty);
			lines.Add(story);
		}
		if (lines.Count == 0) return config.Name;
		return string.Join("\n", lines);
	}
	public readonly ItemFlagCode flag;
	public readonly ItemIdCode id;
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
	public List<Buff> Buffs { get; } = [];
	public bool HasBuff(BuffCode buff, bool recursive)
	{
		foreach (var owned in Buffs)
			if (owned.code == buff)
				return true;
		if (!recursive) return false;
		foreach (var slot in Slots)
		{
			var item = slot.Item;
			if (item == null) continue;
			if (item.HasBuff(buff, true)) return true;
		}
		return false;
	}
	Item(ItemIdCode id, ItemConfig config)
	{
		this.id = id;
		flag = config.Flag;
		Name = config.Name;
		Description = BuildDescription(config);
		Icon = config.Icon;
		Length = config.Length;
		Weight = config.Weight;
		Slots = CreateSlots(config.SlotFlags, this);
		HitPoint = new(config.HitPointMax, config.HitPointMax);
		DamageProfile = config.DamageProfile;
		Protection = config.Protection;
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
