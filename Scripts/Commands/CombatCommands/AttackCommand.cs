using System.Collections.Generic;
using RealismCombat.Data;
using System.Threading.Tasks;
using RealismCombat.StateMachine.CombatStates;
namespace RealismCombat.Commands.CombatCommands;
class AttackCommand : CombatCommand
{
	public const string name = "combat_attack";
	static bool GetBodyPart(CharacterData actor, string partName, out BodyPartData bodyPartData)
	{
		bodyPartData = partName switch
		{
			"head" => actor.head,
			"chest" => actor.chest,
			"leftArm" => actor.leftArm,
			"rightArm" => actor.rightArm,
			"leftLeg" => actor.leftLeg,
			"rightLeg" => actor.rightLeg,
			_ => null!,
		};
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		return bodyPartData != null;
	}
	public readonly ActionState actionState;
	public AttackCommand(ActionState actionState, IReadOnlyDictionary<string, string>? arguments = null) :
		base(combatState: actionState.combatState, arguments: arguments) =>
		this.actionState = actionState;
	public AttackCommand(ActionState actionState, string command) : base(combatState: actionState.combatState, command: command) =>
		this.actionState = actionState;
    public override Task Execute()
	{
		var actor = actionState.actor;
		var name = arguments.GetValueOrDefault(key: "target", defaultValue: "");
		var attackPart = arguments.GetValueOrDefault(key: "attackerPart", defaultValue: "");
		var targetPart = arguments.GetValueOrDefault(key: "targetPart", defaultValue: "");
		if (!FindCharacter(name: name, target: out var target))
		{
			Log.Print($"找不到名为{name}的目标");
			rootNode.McpCheckPoint();
            return Task.CompletedTask;
		}
		if (!GetBodyPart(actor: actor, partName: attackPart, bodyPartData: out _))
		{
			Log.Print($"无效的攻击部位:{attackPart}");
			rootNode.McpCheckPoint();
            return Task.CompletedTask;
		}
		if (!GetBodyPart(actor: target, partName: targetPart, bodyPartData: out var defenderPart))
		{
			Log.Print($"无效的目标部位:{targetPart}");
			rootNode.McpCheckPoint();
            return Task.CompletedTask;
		}
		actor.actionPoint -= 3;
		defenderPart.hp -= 2;
		Log.Print($"{actor.name}用{attackPart}攻击{target.name}的{targetPart}，造成2点伤害");
		Log.Print($"{target.name}的{targetPart}剩余生命值:{defenderPart.hp}");
		if (target.Dead) Log.Print($"{target.name}已死亡");
		_ = new TurnProgressState(combatState: combatState, combatData: combatState.combatData);
        return Task.CompletedTask;
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
