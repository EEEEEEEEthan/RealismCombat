using System.Collections.Generic;
using RealismCombat.Data;
using RealismCombat.StateMachine.CombatStates;
namespace RealismCombat.Commands.CombatCommands;
class AttackCommand : CombatCommand
{
	public readonly ActionState actionState;
	public AttackCommand(ActionState actionState, IReadOnlyDictionary<string, string>? arguments = null) :
		base(combatState: actionState.combatState, arguments: arguments) =>
		this.actionState = actionState;
	public AttackCommand(ActionState actionState, string command) : base(combatState: actionState.combatState, command: command) =>
		this.actionState = actionState;
	public override void Execute()
	{
		var actor = actionState.actor;
		var name = arguments.GetValueOrDefault(key: "target", defaultValue: "");
		var attackPart = arguments.GetValueOrDefault(key: "attacker", defaultValue: "");
		var targetPart = arguments.GetValueOrDefault(key: "defender", defaultValue: "");
		if (!FindCharacter(name: name, target: out var target))
		{
			Log.Print($"找不到名为{name}的目标");
			rootNode.McpCheckPoint();
			return;
		}
		if (!GetBodyPart(actor: actor, partName: attackPart, bodyPart: out var actorPart))
		{
			Log.Print($"无效的攻击部位:{attackPart}");
			rootNode.McpCheckPoint();
			return;
		}
		if (!GetBodyPart(actor: target, partName: targetPart, bodyPart: out var defenderPart))
		{
			Log.Print($"无效的目标部位:{targetPart}");
			rootNode.McpCheckPoint();
			return;
		}
		actor.actionPoint -= 3;
		defenderPart.hp -= 2;
		Log.Print($"{actor.name}用{attackPart}攻击{target.name}的{targetPart}，造成2点伤害");
		Log.Print($"{target.name}的{targetPart}剩余生命值:{defenderPart.hp}");
		if (target.Dead) Log.Print($"{target.name}已死亡");
		_ = new TurnProgressState(combatState: combatState, combatData: combatState.combatData);
	}
	bool GetBodyPart(CharacterData actor, string partName, out BodyPart bodyPart)
	{
		bodyPart = partName switch
		{
			"头部" => actor.head,
			"胸部" => actor.chest,
			"左臂" => actor.leftArm,
			"右臂" => actor.rightArm,
			"左腿" => actor.leftLeg,
			"右腿" => actor.rightLeg,
			_ => null!,
		};
		return bodyPart != null;
	}
	bool FindCharacter(string name, out CharacterData target)
	{
		foreach (var character in combatState.combatData.characters)
			if (character.name == name)
			{
				target = character;
				return true;
			}
		target = null!;
		return false;
	}
}
