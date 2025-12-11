using System;
/// <summary>
///     半剑式攻击，只允许持武器的手臂使用。
///     伤害低于刺击，前摇更长，但更容易击中盔甲缝隙。
/// </summary>
public class HalfSwordAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 3, 4)
{
	public override CombatActionCode Id => CombatActionCode.HalfSword;
	public override string Narrative => "双手握持武器刃部精准刺击目标，造成刺击伤害，更容易击中盔甲缝隙。需要一只手持有长剑，另一只手空闲且有护甲。";
	public override string PreCastText => $"{actor.name}双手握持{actorBodyPart.Name}的武器刃部开始蓄力...";
	public override string CastText => $"{actor.name}用半剑式刺击{target!.name}的{targetObject!.Name}!";
	public override bool Visible => actorBodyPart is { Available: true, id.IsArm: true, };
	public override bool Disabled
	{
		get
		{
			if (actorBodyPart is not { Available: true, id.IsArm: true, }) return true;
			// 必须持有长剑且武器可用
			if (!HasAvailableLongSword(actorBodyPart)) return true;
			// 另一只手必须为空且有护甲
			var otherArm = actorBodyPart.id == BodyPartCode.LeftArm ? actor.rightArm : actor.leftArm;
			if (otherArm.HasWeapon) return true;
			// 检查另一只手是否有护甲
			foreach (var item in otherArm.IterItems(ItemFlagCode.Armor))
				return false;
			return true;
		}
	}
	public override double DodgeImpact
	{
		get
		{
			if (targetObject is BodyPart targetPart)
			{
				var heightGap = Math.Abs(actorBodyPart.id.NormalizedHeight - targetPart.id.NormalizedHeight);
				if (TryGetWeaponLength(out var weaponLength))
					heightGap = Math.Abs(heightGap - weaponLength);
				if (heightGap >= 0.4) return 0.95;
			}
			return 0.75;
		}
	}
	public override double BlockImpact
	{
		get
		{
			if (targetObject is BodyPart targetPart)
			{
				var heightGap = Math.Abs(actorBodyPart.id.NormalizedHeight - targetPart.id.NormalizedHeight);
				if (TryGetWeaponLength(out var weaponLength))
					heightGap = Math.Abs(heightGap - weaponLength);
				if (heightGap >= 0.4) return 0.60;
			}
			return 0.30;
		}
	}
	public override AttackTypeCode AttackType => AttackTypeCode.Thrust;
	public override bool UsesWeapon => true;
	public override double ArmorGapPower => 2.0;
	public override Damage Damage
	{
		get
		{
			var baseDamage = base.Damage;
			// 伤害为刺击的80%
			return baseDamage * 0.8f;
		}
	}
	protected override bool IsBodyPartUsable(BodyPart bodyPart)
	{
		// 不再使用此方法，使用Visible和Disabled属性代替
		return true;
	}
	bool HasAvailableLongSword(BodyPart bodyPart)
	{
		foreach (var slot in bodyPart.Slots)
		{
			var weapon = slot.Item;
			if (weapon == null) continue;
			if (!weapon.Available) continue;
			if ((weapon.flag & ItemFlagCode.Arm) == 0) continue;
			if (weapon.id == ItemIdCode.LongSword) return true;
		}
		return false;
	}
	bool TryGetWeaponLength(out double length)
	{
		foreach (var slot in actorBodyPart.Slots)
		{
			var weapon = slot.Item;
			if (weapon == null) continue;
			if ((weapon.flag & ItemFlagCode.Arm) == 0) continue;
			length = weapon.Length;
			return true;
		}
		length = 0.0;
		return false;
	}
}
