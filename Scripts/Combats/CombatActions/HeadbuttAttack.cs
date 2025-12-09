using System;
/// <summary>
///     头槌攻击，只允许头使用
/// </summary>
public class HeadbuttAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 1, 4)
{
	public override CombatActionCode Id => CombatActionCode.Headbutt;
	public override string Narrative => "用头部进行头槌攻击，近距离的特殊冲撞";
	public override string PreCastText => $"{actor.name}头向后仰!";
	public override string CastText => $"{actor.name}用{actorBodyPart.Name}头槌{target!.name}的{targetObject!.Name}!";
	public override double DodgeImpact
	{
		get
		{
			if (targetObject is BodyPart targetPart)
			{
				var heightGap = Math.Abs(actorBodyPart.id.NormalizedHeight - targetPart.id.NormalizedHeight);
				if (heightGap >= 0.4) return 0.95;
			}
			return 0.35;
		}
	}
	public override double BlockImpact
	{
		get
		{
			if (targetObject is BodyPart targetPart)
			{
				var heightGap = Math.Abs(actorBodyPart.id.NormalizedHeight - targetPart.id.NormalizedHeight);
				if (heightGap >= 0.4) return 0.9;
			}
			return 0.35;
		}
	}
	public override AttackTypeCode AttackType => AttackTypeCode.Special;
	public override Damage Damage => new(0f, 0f, 1f);
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id: BodyPartCode.Head, };
}
