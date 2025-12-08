using System.Collections.Generic;
using Godot;

/// <summary>
///     负责根据攻击与防护计算最终伤害
/// </summary>
public static class DamageResolver
{
	/// <summary>
	///     根据护甲覆盖率决定最终命中的目标与防护
	/// </summary>
	public static (ICombatTarget target, Protection protection) ResolveTarget(ICombatTarget target)
	{
		if (target is BodyPart bodyPart)
		{
			var armors = new List<Item>();
			foreach (var slot in bodyPart.Slots)
			{
				var armorItem = slot.Item;
				if (armorItem == null) continue;
				if ((armorItem.flag & (ItemFlagCode.TorsoArmor | ItemFlagCode.HandArmor | ItemFlagCode.LegArmor)) != 0)
					armors.Add(armorItem);
			}
			if (armors.Count > 0)
			{
				var startIndex = (int)(GD.Randi() % (uint)armors.Count);
				for (var i = 0; i < armors.Count; i++)
				{
					var armor = armors[(startIndex + i) % armors.Count];
					if (armor.Coverage <= 0.0) continue;
					if (GD.Randf() < armor.Coverage) return (armor, armor.Protection);
				}
			}
			return (bodyPart, Protection.Zero);
		}
		if (target is Item item) return (item, item.Protection);
		return (target, Protection.Zero);
	}
	public static Damage GetBaseDamage(AttackBase attack)
	{
		var attackType = attack.AttackType;
		if (attack.UsesWeapon)
		{
			foreach (var slot in attack.ActorBodyPart.Slots)
			{
				var weapon = slot.Item;
				if (weapon != null && (weapon.flag & ItemFlagCode.Arm) != 0)
					return weapon.DamageProfile.Get(attackType);
			}
		}
		return attackType switch
		{
			AttackTypeCode.Special => new Damage(0f, 0f, 1f),
			_ => Damage.Zero,
		};
	}
}

