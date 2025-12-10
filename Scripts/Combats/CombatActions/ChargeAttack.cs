using System;
using System.Collections.Generic;
using System.Threading.Tasks;
/// <summary>
///     撞击攻击，只允许躯干使用
/// </summary>
public class ChargeAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 1, 4)
{
	public override CombatActionCode Id => CombatActionCode.Charge;
	public override string Narrative => "用躯干蓄力撞击目标，造成特殊近战攻击";
	public override IEnumerable<(ICombatTarget target, bool disabled)> AvailableTargetObjects
	{
		get
		{
			var target = this.target;
			if (target == null) yield break;
			foreach (var combatTarget in target.AvailableCombatTargets)
			{
				if (combatTarget is BodyPart { id: BodyPartCode.Head }) continue;
				if (combatTarget is BodyPart bodyPart)
				{
					yield return (bodyPart, !bodyPart.Available);
				}
			}
		}
	}
	public override double DodgeImpact
	{
		get
		{
			if (targetObject is BodyPart targetPart)
			{
				var heightGap = Math.Abs(actorBodyPart.id.NormalizedHeight - targetPart.id.NormalizedHeight);
				if (heightGap >= 0.4) return 0.95;
			}
			return 0.4;
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
			return 0.45;
		}
	}
	public override AttackTypeCode AttackType => AttackTypeCode.Special;
	public override string PreCastText => $"{actor.name}肩膀下沉...";
	public override string CastText => $"{actor.name}用肩膀撞击{target!.name}的{targetObject!.Name}!";
	public override Damage Damage => new(0f, 0f, 1f);
	protected override async Task OnExecute()
	{
		if (targetObject is BodyPart { id: BodyPartCode.LeftLeg or BodyPartCode.RightLeg })
		{
			actor.torso.Buffs.Add(new(BuffCode.Prone, new(actor, targetObject)));
		}
		await base.OnExecute();
	}
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id: BodyPartCode.Torso, };
}
