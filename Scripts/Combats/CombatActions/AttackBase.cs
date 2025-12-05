using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
/// <summary>
///     攻击基类
/// </summary>
public abstract class AttackBase(Character actor, BodyPart actorBodyPart, Character target, ICombatTarget combatTarget, Combat combat)
	: CombatAction(actor, combat, 3, 3)
{
	/// <summary>
	///     检查身体部位是否有武器
	/// </summary>
	protected static bool HasWeapon(BodyPart bodyPart)
	{
		foreach (var slot in bodyPart.Slots)
			if (slot.Item is IArm)
				return true;
		return false;
	}
	/// <summary>
	///     检查身体部位是否是手臂
	/// </summary>
	protected static bool IsArm(BodyPartCode bodyPartCode) => bodyPartCode is BodyPartCode.LeftArm or BodyPartCode.RightArm;
	/// <summary>
	///     检查身体部位是否是腿
	/// </summary>
	protected static bool IsLeg(BodyPartCode bodyPartCode) => bodyPartCode is BodyPartCode.LeftLeg or BodyPartCode.RightLeg;
	public Character Actor => actor;
	public BodyPart ActorBodyPart => actorBodyPart;
	public Character Target => target;
	public ICombatTarget CombatTarget => combatTarget;
	/// <summary>
	///     获取攻击开始时的对话文本
	/// </summary>
	protected abstract string GetStartDialogueText();
	/// <summary>
	///     获取攻击执行时的对话文本
	/// </summary>
	protected abstract string GetExecuteDialogueText();
	/// <summary>
	///     计算伤害值
	/// </summary>
	protected abstract int CalculateDamage();
	protected override async Task OnStartTask() => await DialogueManager.CreateGenericDialogue(GetStartDialogueText());
	protected override async Task OnExecute()
	{
		var actorNode = combat.combatNode.GetCharacterNode(actor);
		var targetNode = combat.combatNode.GetCharacterNode(target);
		var actorPosition = combat.combatNode.GetPKPosition(actor);
		var targetPosition = combat.combatNode.GetPKPosition(target);
		using var _ = actorNode.MoveScope(actorPosition);
		using var __ = targetNode.MoveScope(targetPosition);
		using var ___ = actorNode.ExpandScope();
		using var ____ = targetNode.ExpandScope();
		var startDialogue = DialogueManager.CreateGenericDialogue(GetExecuteDialogueText());
		await startDialogue;
		var reaction = await combat.HandleIncomingAttack(this);
		var finalTarget = combatTarget;
		var attackHit = true;
		var resultMessages = new List<string>();
		var hitPosition = combat.combatNode.GetHitPosition(actor);
		actorNode.MoveTo(hitPosition);
		switch (reaction.Type)
		{
			case ReactionType.Dodge:
				await Task.Delay(10);
				targetNode.MoveTo(combat.combatNode.GetDogePosition(target));
				resultMessages.Add($"{target.name}及时闪避, 攻击落空");
				attackHit = false;
				break;
			case ReactionType.Block:
				await Task.Delay(50);
				targetNode.MoveTo(targetPosition + Vector2.Up * 12);
				targetNode.FlashFrame();
				await Task.Delay(100);
				targetNode.MoveTo(targetPosition);
				finalTarget = reaction.BlockTarget!;
				AudioManager.PlaySfx(ResourceTable.blockSound, 6f);
				resultMessages.Add($"{target.name}使用{finalTarget.Name}进行了格挡");
				await Task.Delay((int)(ResourceTable.blockSound.Value.GetLength() * 1000));
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
			var damage = CalculateDamage();
			finalTarget.HitPoint.value = Mathf.Clamp(finalTarget.HitPoint.value - damage, 0, finalTarget.HitPoint.maxValue);
			targetNode.FlashPropertyNode(finalTarget);
			resultMessages.Add($"{target.name}的{finalTarget.Name}受到了{damage}点伤害，剩余{finalTarget.HitPoint.value}/{finalTarget.HitPoint.maxValue}");
			if (!finalTarget.Available)
				resultMessages.Add(finalTarget is BodyPart ? $"{target.name}的{finalTarget.Name}失去战斗能力" : $"{target.name}的{finalTarget.Name}已无法继续使用");
			if (!target.IsAlive) resultMessages.Add($"{target.name}倒下了");
			await OnAttackHit(finalTarget, resultMessages);
		}
		else if (resultMessages.Count == 0)
		{
			resultMessages.Add($"{target.name}成功避开了攻击");
		}
		var resultDialogue = DialogueManager.CreateGenericDialogue(resultMessages.ToArray());
		await resultDialogue;
		actor.actionPoint.value = Math.Max(0, actor.actionPoint.value - 5);
	}
	/// <summary>
	///     攻击命中后的额外效果，子类可以重写
	/// </summary>
	protected virtual Task OnAttackHit(ICombatTarget finalTarget, List<string> resultMessages) => Task.CompletedTask;
}
