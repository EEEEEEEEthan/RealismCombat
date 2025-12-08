using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
/// <summary>
///     攻击基类
/// </summary>
public abstract class AttackBase(Character actor, BodyPart actorBodyPart, Combat combat, double preCastActionPointCost, double postCastActionPointCost)
	: CombatAction(actor, combat, actorBodyPart, preCastActionPointCost, postCastActionPointCost)
{
	internal abstract double DodgeImpact { get; }
	internal abstract double BlockImpact { get; }
	internal abstract AttackTypeCode AttackType { get; }
	internal virtual double DamageMultiplier => 1.0;
	internal virtual bool UsesWeapon => false;
	Character? target;
	ICombatTarget? combatTarget;
	protected readonly BodyPart actorBodyPart = actorBodyPart;
	public BodyPart ActorBodyPart => actorBodyPart;
	public override abstract CombatActionCode Code { get; }
public override abstract string Description { get; }
	public override bool Available => actorBodyPart.Available && IsBodyPartUsable(actorBodyPart);
	public override IEnumerable<Character> AvailableTargets => GetOpponents().Where(c => c.IsAlive);
	public override IEnumerable<(ICombatTarget target, bool disabled)> AvailableTargetObjects
	{
		get
		{
			var target = Target;
			if (target == null) return Array.Empty<(ICombatTarget, bool)>();
			return GetAvailableTargets(target).Select(t => (t, !t.Available));
		}
	}
	public override Character? Target
	{
		get => target;
		set => target = value;
	}
	public override ICombatTarget? TargetObject
	{
		get => combatTarget;
		set => combatTarget = value;
	}
	public ICombatTarget CombatTarget => TargetCombatObject;
	protected virtual bool ShouldResolveDamage => true;
	public virtual Damage GetPreviewDamage() => CalculateDamage();
	protected string BuildAttackDescription(string narrative)
	{
		var typeText = AttackType switch
		{
			AttackTypeCode.Swing => "挥砍",
			AttackTypeCode.Thrust => "刺击",
			AttackTypeCode.Special => "特殊",
			_ => "未知",
		};
		static string DodgeText(double impact) => impact switch
		{
			>= 0.6 => "容易被闪避",
			>= 0.4 => "中等闪避难度",
			_ => "不易被闪避",
		};
		static string BlockText(double impact) => impact switch
		{
			>= 0.6 => "容易被格挡",
			>= 0.4 => "中等格挡难度",
			_ => "不易被格挡",
		};
		return $"类型: {typeText}\n闪避倾向: {DodgeText(DodgeImpact)}\n格挡倾向: {BlockText(BlockImpact)}\n{narrative}";
	}
	protected static bool HasWeapon(BodyPart bodyPart)
	{
		foreach (var slot in bodyPart.Slots)
			if (slot.Item != null && (slot.Item.flag & ItemFlagCode.Arm) != 0)
				return true;
		return false;
	}
	protected static bool IsArm(BodyPartCode bodyPartCode) => bodyPartCode is BodyPartCode.LeftArm or BodyPartCode.RightArm;
	protected static bool IsLeg(BodyPartCode bodyPartCode) => bodyPartCode is BodyPartCode.LeftLeg or BodyPartCode.RightLeg;
	protected abstract bool IsBodyPartUsable(BodyPart bodyPart);
protected Character TargetCharacter => target ?? throw new InvalidOperationException("攻击未设置目标角色");
protected ICombatTarget TargetCombatObject => combatTarget ?? throw new InvalidOperationException("攻击未设置目标对象");
	protected abstract string GetStartDialogueText();
	protected abstract string GetExecuteDialogueText();
	protected virtual Damage CalculateDamage() => DamageResolver.GetBaseDamage(this).Scale(DamageMultiplier);
	protected override async Task OnStartTask() => await DialogueManager.ShowGenericDialogue(GetStartDialogueText());
	protected override async Task OnExecute()
	{
		var target = TargetCharacter;
		var combatTarget = TargetCombatObject;
		var actorNode = combat.combatNode.GetCharacterNode(actor);
		var targetNode = combat.combatNode.GetCharacterNode(target);
		var actorPosition = combat.combatNode.GetPKPosition(actor);
		var targetPosition = combat.combatNode.GetPKPosition(target);
		using var _ = actorNode.MoveScope(actorPosition);
		using var __ = targetNode.MoveScope(targetPosition);
		using var ___ = actorNode.ExpandScope();
		using var ____ = targetNode.ExpandScope();
		await DialogueManager.ShowGenericDialogue(GetExecuteDialogueText());
		var reaction = await combat.HandleIncomingAttack(this);
		var reactionOutcome = ReactionSuccessCalculator.Resolve(reaction, this);
		var finalTarget = combatTarget;
		var attackHit = true;
		var resultMessages = new List<string>();
		var hitPosition = combat.combatNode.GetHitPosition(actor);
		actorNode.MoveTo(hitPosition);
		switch (reactionOutcome.Type)
		{
			case ReactionType.Dodge when reactionOutcome.Succeeded:
				await Task.Delay(10);
				targetNode.MoveTo(combat.combatNode.GetDogePosition(target));
				resultMessages.Add($"{target.name}闪避成功");
				attackHit = false;
				break;
			case ReactionType.Dodge:
				resultMessages.Add($"{target.name}尝试闪避但失败");
				await Task.Delay(100);
				targetNode.Shake();
				AudioManager.PlaySfx(ResourceTable.retroHurt1);
				break;
			case ReactionType.Block when reactionOutcome is { Succeeded: true, BlockTarget: not null, }:
				await Task.Delay(50);
				targetNode.MoveTo(targetPosition + Vector2.Up * 12);
				targetNode.FlashFrame();
				await Task.Delay(100);
				targetNode.MoveTo(targetPosition);
				finalTarget = reactionOutcome.BlockTarget ?? combatTarget;
				AudioManager.PlaySfx(ResourceTable.blockSound, 6f);
				resultMessages.Add($"{target.name}使用{finalTarget.Name}格挡成功");
				await Task.Delay((int)(ResourceTable.blockSound.Value.GetLength() * 1000));
				break;
			case ReactionType.Block:
				resultMessages.Add($"{target.name}尝试格挡但失败");
				await Task.Delay(100);
				targetNode.Shake();
				AudioManager.PlaySfx(ResourceTable.retroHurt1);
				break;
			case ReactionType.None:
				await Task.Delay(100);
				targetNode.Shake();
				AudioManager.PlaySfx(ResourceTable.retroHurt1);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		actorNode.MoveTo(actorPosition);
		if (attackHit)
		{
			if (ShouldResolveDamage)
			{
				var rawDamage = CalculateDamage();
				var protection = DamageResolver.GetProtection(finalTarget);
				var mitigatedDamage = rawDamage.ApplyProtection(protection);
				var damageValue = Mathf.CeilToInt(mitigatedDamage.Total);
				if (damageValue > 0)
				{
					finalTarget.HitPoint.value = Mathf.Clamp(finalTarget.HitPoint.value - damageValue, 0, finalTarget.HitPoint.maxValue);
					targetNode.FlashPropertyNode(finalTarget);
					if (finalTarget is not Item)
					{
						resultMessages.Add($"{target.name}的{finalTarget.Name}受到了{damageValue}点伤害，剩余{finalTarget.HitPoint.value}/{finalTarget.HitPoint.maxValue}");
					}
					else
					{
						resultMessages.Add($"{target.name}的{finalTarget.Name}受到了{damageValue}点伤害");
					}
					if (!finalTarget.Available)
						resultMessages.Add(finalTarget is BodyPart ? $"{target.name}的{finalTarget.Name}失去战斗能力" : $"{target.name}的{finalTarget.Name}已无法继续使用");
					if (!target.IsAlive) resultMessages.Add($"{target.name}倒下了");
					await OnAttackHit(finalTarget, resultMessages);
				}
				else
				{
					resultMessages.Add($"{target.name}的{finalTarget.Name}被防护抵消了伤害");
				}
			}
			else
			{
				await OnAttackHit(finalTarget, resultMessages);
				if (resultMessages.Count == 0)
					resultMessages.Add($"{actor.name}的攻击未造成显著效果");
			}
		}
		else if (resultMessages.Count == 0)
		{
			resultMessages.Add($"{target.name}成功避开了攻击");
		}
		await DialogueManager.ShowGenericDialogue(resultMessages);
		actor.actionPoint.value = Math.Max(0, actor.actionPoint.value - 5);
	}
	protected virtual Task OnAttackHit(ICombatTarget finalTarget, List<string> resultMessages) => Task.CompletedTask;
	static ICombatTarget[] GetAvailableTargets(Character character) =>
		character.bodyParts.Where(part => part.Available).Cast<ICombatTarget>().ToArray();
	IEnumerable<Character> GetOpponents() => combat.Allies.Contains(actor) ? combat.Enemies : combat.Allies;
}
