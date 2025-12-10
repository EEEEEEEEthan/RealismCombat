using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
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
			if (!actor.torso.HasBuff(BuffCode.Prone, false))
			{
				actor.torso.Buffs[BuffCode.Prone] = new(BuffCode.Prone, new(actor, targetObject));
				await DialogueManager.ShowGenericDialogue($"{actor.name}失去平衡倒下了!");
			}
		}
		await base.OnExecute();
	}
	protected override async Task OnAttackLanded(Character targetCharacter, ICombatTarget targetObject, GenericDialogue dialogue)
	{
		// 撞击命中时，按重量比计算目标获得倒伏buff的概率
		var targetWeight = targetCharacter.TotalWeight;
		var actorWeight = actor.TotalWeight;
		var proneChance = targetWeight / actorWeight;
		
		if (GD.Randf() < proneChance)
		{
			var source = new BuffSource(actor, actorBodyPart);
			if (!targetCharacter.torso.HasBuff(BuffCode.Prone, false))
			{
				targetCharacter.torso.Buffs[BuffCode.Prone] = new(BuffCode.Prone, source);
				await dialogue.ShowTextTask($"{targetCharacter.name}失去平衡倒下了!");
			}
		}
	}
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id: BodyPartCode.Torso, };
}
