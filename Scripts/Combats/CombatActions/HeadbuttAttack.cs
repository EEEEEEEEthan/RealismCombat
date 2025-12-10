using System;
using System.Threading.Tasks;
using Godot;
/// <summary>
///     头槌攻击，只允许头使用
/// </summary>
public class HeadbuttAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 1, 4)
{
	public override CombatActionCode Id => CombatActionCode.Headbutt;
	public override string Narrative => "用头颈进行头槌攻击，近距离的特殊冲撞";
	public override string PreCastText => $"{actor.name}头向后仰!";
	public override string CastText => $"{actor.name}用{actorBodyPart.Name}头槌{target!.name}的{targetObject!.Name}!";
	public override double DodgeImpact
	{
		get
		{
			if (targetObject is BodyPart targetPart)
			{
				// 头槌攻击腿部或手臂时，目标闪避率大幅提高
				if (targetPart.id.IsLeg) return 0.98;
				if (targetPart.id.IsArm) return 0.95;
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
				// 头槌攻击腿部或手臂时，目标格挡率大幅提高
				if (targetPart.id.IsLeg) return 0.95;
				if (targetPart.id.IsArm) return 0.92;
			}
			return 0.35;
		}
	}
	public override AttackTypeCode AttackType => AttackTypeCode.Special;
	public override Damage Damage => new(0f, 0f, 1f);
	protected override async Task OnAttackLanded(Character targetCharacter, ICombatTarget targetObject, GenericDialogue dialogue)
	{
		if (targetObject is BodyPart targetPart)
		{
			var source = new BuffSource(actor, actorBodyPart);
			// 目标是腿时，发起方必定获得倒伏
			if (targetPart.id.IsLeg)
			{
				if (!actor.torso.HasBuff(BuffCode.Prone, false))
				{
					actor.torso.Buffs[BuffCode.Prone] = new(BuffCode.Prone, source);
					await dialogue.ShowTextTask($"{actor.name}头槌腿部导致自己失去平衡倒下了!");
				}
			}
			// 目标是手臂或躯干时，有概率令目标获得倒伏
			else if (targetPart.id.IsArm || targetPart.id == BodyPartCode.Torso)
			{
				var targetWeight = targetCharacter.TotalWeight;
				var actorWeight = actor.TotalWeight;
				var proneChance = targetWeight / actorWeight;
				
				if (GD.Randf() < proneChance)
				{
					if (!targetCharacter.torso.HasBuff(BuffCode.Prone, false))
					{
						targetCharacter.torso.Buffs[BuffCode.Prone] = new(BuffCode.Prone, source);
						await dialogue.ShowTextTask($"{targetCharacter.name}被头槌撞倒了!");
					}
				}
			}
		}
	}
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id: BodyPartCode.Head, };
}
