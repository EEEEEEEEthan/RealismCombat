using System;
using System.Collections.Generic;
using System.Threading.Tasks;
/// <summary>
///     抓取攻击，只允许没有武器的手臂使用
/// </summary>
public class GrabAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 1, 4)
{
	public override CombatActionCode Id => CombatActionCode.Grab;
	public override string Narrative => "徒手擒拿目标，命中可使目标被束缚并让自身进入擒拿状态";
	public override string PreCastText => $"{actor.name}抬起{actorBodyPart.Name}准备抓住目标!";
	public override string CastText => $"{actor.name}用{actorBodyPart.Name}抓取{target!.name}的{targetObject!.Name}!";
	public override double DodgeImpact
	{
		get
		{
			if (targetObject is BodyPart targetPart)
			{
				var heightGap = Math.Abs(actorBodyPart.id.NormalizedHeight - targetPart.id.NormalizedHeight);
				if (heightGap >= 0.4) return 0.98;
			}
			return 0.85;
		}
	}
	public override double BlockImpact
	{
		get
		{
			if (targetObject is BodyPart targetPart)
			{
				var heightGap = Math.Abs(actorBodyPart.id.NormalizedHeight - targetPart.id.NormalizedHeight);
				if (heightGap >= 0.4) return 0.95;
			}
			return 0.65;
		}
	}
	public override AttackTypeCode AttackType => AttackTypeCode.Special;
	public override Damage Damage => Damage.Zero;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id.IsArm: true, HasWeapon: false, };
	protected override async Task OnAttackLanded(Character targetCharacter, ICombatTarget targetObject, GenericDialogue dialogue)
	{
		var addedGrappling = ApplyGrapplingBuff(targetCharacter, targetObject);
		var addedRestrained = ApplyRestrainedBuff(targetObject);
		if (!addedGrappling && !addedRestrained) return;
		var messages = new List<string>();
		if (addedGrappling) messages.Add($"{actor.name}擒拿住了{targetCharacter.name}的{targetObject.Name}");
		if (addedRestrained) messages.Add($"{targetCharacter.name}的{targetObject.Name}被束缚");
		if (messages.Count > 0) await dialogue.ShowTextTask(string.Join("；", messages));
	}
	bool ApplyGrapplingBuff(Character targetCharacter, ICombatTarget targetObject)
	{
		var source = new BuffSource(targetCharacter, targetObject);
		if (HasBuff(actorBodyPart, BuffCode.Grappling, source)) return false;
		actorBodyPart.Buffs.Add(new(BuffCode.Grappling, source));
		return true;
	}
	bool ApplyRestrainedBuff(ICombatTarget targetObject)
	{
		if (targetObject is not IBuffOwner buffOwner) return false;
		var source = new BuffSource(actor, actorBodyPart);
		if (HasBuff(buffOwner, BuffCode.Restrained, source)) return false;
		buffOwner.Buffs.Add(new(BuffCode.Restrained, source));
		return true;
	}
	static bool HasBuff(IBuffOwner owner, BuffCode code, BuffSource source)
	{
		foreach (var buff in owner.Buffs)
			if (buff.code == code && buff.source is { } buffSource && buffSource == source)
				return true;
		return false;
	}
}
