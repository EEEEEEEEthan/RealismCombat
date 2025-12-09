using System;
/// <summary>
///     刺击攻击，只允许持武器的手臂使用
/// </summary>
public class StabAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 2, 4)
{
	public override CombatActionCode Id => CombatActionCode.Stab;
	public override string Narrative => "持武器刺击目标，造成刺击伤害，依赖手部武器";
	public override string PreCastText => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	public override string CastText => $"{actor.name}用{actorBodyPart.Name}刺击{target!.name}的{targetObject!.Name}!";
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
			return 0.7;
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
				if (heightGap >= 0.4) return 0.9;
			}
			return 0.35;
		}
	}
	public override AttackTypeCode AttackType => AttackTypeCode.Thrust;
	public override bool UsesWeapon => true;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id.IsArm: true, HasWeapon: true, };
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
