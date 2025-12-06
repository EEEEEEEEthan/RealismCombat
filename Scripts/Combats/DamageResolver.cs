using System;

/// <summary>
///     负责根据攻击与防护计算最终伤害
/// </summary>
public static class DamageResolver
{
	public static Damage Calculate(AttackBase attack, ICombatTarget target)
	{
		var baseDamage = GetBaseDamage(attack);
		var scaledDamage = baseDamage.Scale(attack.DamageMultiplier);
		var protection = GetProtection(target);
		return scaledDamage.ApplyProtection(protection);
	}
	public static Protection GetProtection(ICombatTarget target)
	{
		switch (target)
		{
			case Item item:
				return item.Protection;
			case BodyPart bodyPart:
				return GetBodyPartProtection(bodyPart);
			default:
				return Protection.Zero;
		}
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

