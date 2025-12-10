using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
/// <summary>
///     测试用处决行动，徒手将敌方指定部位生命降至零
/// </summary>
public class ExecutionAction(Character actor, BodyPart actorBodyPart, Combat combat)
	: CombatAction(actor, combat, actorBodyPart, 0, 0)
{
	Character? target;
	BodyPart? targetBodyPart;
	bool executed;
	public override string Description => "测试: 直接让敌方任意部位生命归零";
	public override bool Visible => actorBodyPart is { Available: true, id.IsArm: true, };
	public virtual CombatActionCode Id => CombatActionCode.Execution;
	public virtual IEnumerable<Character> AvailableTargets => Opponents.Where(c => c.IsAlive);
	public virtual IEnumerable<(ICombatTarget target, bool disabled)> AvailableTargetObjects
	{
		get
		{
			var opponent = target;
			if (opponent == null) return [];
			return opponent.bodyParts.Select(bp => ((ICombatTarget)bp, false));
		}
	}
	public virtual Character? Target
	{
		get => target;
		set => target = value;
	}
	public virtual ICombatTarget? TargetObject
	{
		get => targetBodyPart;
		set => targetBodyPart = value as BodyPart;
	}
	IEnumerable<Character> Opponents => combat.Allies.Contains(actor) ? combat.Enemies : combat.Allies;
	protected override async Task OnStartTask()
	{
		await DialogueManager.ShowGenericDialogue($"{actor.name}举起{actorBodyPart.Name}准备处决");
		await Execute();
	}
	protected override async Task OnExecute()
	{
		if (executed) return;
		await Execute();
	}
	async Task Execute()
	{
		if (target == null || targetBodyPart == null)
		{
			target = Opponents.FirstOrDefault(c => c.IsAlive);
			targetBodyPart = target?.bodyParts.FirstOrDefault();
			if (target == null || targetBodyPart == null)
			{
				await DialogueManager.ShowGenericDialogue("处决目标未选择");
				return;
			}
		}
		executed = true;
		targetBodyPart.HitPoint.value = 0;
		var targetNode = combat.combatNode.GetCharacterNode(target);
		targetNode.FlashPropertyNode(targetBodyPart);
		targetNode.Shake();
		AudioManager.PlaySfx(ResourceTable.retroHurt1);
		var message = $"{actor.name}用{actorBodyPart.Name}处决了{target.name}的{targetBodyPart.Name}";
		if (!target.IsAlive) message += $"，{target.name}倒下了";
		Log.Print($"[处决] {message} 剩余{targetBodyPart.HitPoint.value}/{targetBodyPart.HitPoint.maxValue}");
		await DialogueManager.ShowGenericDialogue(message);
		actor.combatAction = null;
	}
}
