using System;
using System.Collections.Generic;
using Godot;

/// <summary>
///     负责根据攻击与防护计算最终伤害
/// </summary>
public static class DamageResolver
{
	public static Damage Calculate(AttackBase attack, ICombatTarget target)
	{
		var baseDamage = GetBaseDamage(attack);
		var scaledDamage = baseDamage.Scale(attack.DamageMultiplier);
		var (_, protection) = ResolveTarget(target);
		return scaledDamage.ApplyProtection(protection);
	}
	/// <summary>
	///     根据护甲覆盖率决定最终命中的目标与防护
	/// </summary>
	public static (ICombatTarget target, Protection protection) ResolveTarget(ICombatTarget target)
	{
		if (target is BodyPart bodyPart)
		{
			var armors = GetArmorItems(bodyPart);
			var selectedArmor = SelectArmorByCoverage(armors);
			if (selectedArmor != null) return (selectedArmor, selectedArmor.Protection);
			return (bodyPart, Protection.Zero);
		}
		return (target, GetProtection(target));
	}
	public static Protection GetProtection(ICombatTarget target)
	{
		return target switch
		{
			Item item => item.Protection,
			BodyPart bodyPart => GetBodyPartProtection(bodyPart),
			_ => Protection.Zero,
		};
	}
	public static Damage GetBaseDamage(AttackBase attack)
	{
		var attackType = attack.AttackType;
		if (attack.UsesWeapon)
		{
			var weapon = GetWeaponInUse(attack.ActorBodyPart);
			if (weapon != null) return weapon.DamageProfile.Get(attackType);
		}
		return GetUnarmedDamage(attackType);
	}
	static Protection GetBodyPartProtection(BodyPart bodyPart)
	{
		var protection = Protection.Zero;
		foreach (var slot in bodyPart.Slots)
		{
			if (slot.Item == null) continue;
			protection = protection.Add(slot.Item.Protection);
		}
		return protection;
	}
	static List<Item> GetArmorItems(BodyPart bodyPart)
	{
		var armors = new List<Item>();
		foreach (var slot in bodyPart.Slots)
		{
			if (slot.Item == null) continue;
			var item = slot.Item;
			if (IsArmor(item)) armors.Add(item);
		}
		return armors;
	}
	static Item? SelectArmorByCoverage(IReadOnlyList<Item> armors)
	{
		if (armors.Count == 0) return null;
		var totalCoverage = 0.0;
		foreach (var armor in armors) totalCoverage += armor.Coverage;
		if (totalCoverage <= 0.0) return null;
		var coverageChance = Math.Min(1.0, totalCoverage);
		if (GD.Randf() > coverageChance) return null;
		var roll = GD.Randf() * totalCoverage;
		foreach (var armor in armors)
		{
			roll -= armor.Coverage;
			if (roll <= 0.0) return armor;
		}
		return armors[^1];
	}
	static bool IsArmor(Item item) =>
		(item.flag & (ItemFlagCode.TorsoArmor | ItemFlagCode.HandArmor | ItemFlagCode.LegArmor)) != 0;
	static Item? GetWeaponInUse(BodyPart bodyPart)
	{
		foreach (var slot in bodyPart.Slots)
		{
			if (slot.Item != null && (slot.Item.flag & ItemFlagCode.Arm) != 0) return slot.Item;
		}
		return null;
	}
	static Damage GetUnarmedDamage(AttackTypeCode attackType) =>
		attackType switch
		{
			AttackTypeCode.Special => new Damage(0f, 0f, 1f),
			_ => Damage.Zero,
		};
}

