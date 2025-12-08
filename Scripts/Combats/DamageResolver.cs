
/// <summary>
///     负责根据攻击与防护计算最终伤害
/// </summary>
public static class DamageResolver
{
	/// <summary>
	///     负责根据攻击与防护计算最终伤害
	/// </summary>
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

