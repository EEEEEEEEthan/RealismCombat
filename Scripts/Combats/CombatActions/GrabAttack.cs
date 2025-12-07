using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
/// <summary>
///     抓取攻击，只允许没有武器的手臂使用
/// </summary>
public class GrabAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat)
{
	internal override double DodgeImpact => 0.7;
	internal override double BlockImpact => 0.25;
	internal override AttackTypeCode AttackType => AttackTypeCode.Special;
	protected override bool ShouldResolveDamage => false;
	public static bool IsBodyPartCompatible(BodyPart bodyPart) => IsArm(bodyPart.id);
	public static bool CanUse(BodyPart bodyPart) => bodyPart.Available && IsBodyPartCompatible(bodyPart) && !HasWeapon(bodyPart);
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => CanUse(bodyPart);
	protected override string GetStartDialogueText() => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string GetExecuteDialogueText() => $"{actor.name}用{actorBodyPart.Name}抓取{TargetCharacter.name}的{TargetCombatObject.Name}!";
	protected override Damage CalculateDamage() => Damage.Zero;
	protected override async Task OnAttackHit(ICombatTarget finalTarget, List<string> resultMessages)
	{
		var grabSuccessChance = 0.5f;
		var target = TargetCharacter;
		if (GD.Randf() < grabSuccessChance)
		{
			if (target.torso is IBuffOwner targetTorsoBuffOwner)
			{
				var restrainedBuff = new Buff(BuffCode.Restrained, actor);
				targetTorsoBuffOwner.AddBuff(restrainedBuff);
				resultMessages.Add($"{target.name}的{finalTarget.Name}被{actor.name}抓住了!");
			}
			if (actorBodyPart is IBuffOwner actorBuffOwner)
			{
				var grapplingBuff = new Buff(BuffCode.Grappling, actor);
				actorBuffOwner.AddBuff(grapplingBuff);
				resultMessages.Add($"{actor.name}的{actorBodyPart.Name}正在擒拿{target.name}!");
			}
		}
		await Task.CompletedTask;
	}
}
