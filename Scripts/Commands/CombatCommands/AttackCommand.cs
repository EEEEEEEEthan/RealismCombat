using System;
using System.Collections.Generic;
using RealismCombat.Data;
using System.Threading.Tasks;
using RealismCombat.Nodes;
using RealismCombat.StateMachine.CombatStates;
namespace RealismCombat.Commands.CombatCommands;
class AttackCommand : CombatCommand
{
	public const string name = "combat_attack";
	/// <summary>
	/// 	默认命中率（0-1）
	/// </summary>
	private const float DefaultHitChance = 0.8f;
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
    public override async Task Execute()
	{
		var actor = actionState.actor;
		var name = arguments.GetValueOrDefault(key: "target", defaultValue: "");
		var attackPart = arguments.GetValueOrDefault(key: "attackerPart", defaultValue: "");
		var targetPart = arguments.GetValueOrDefault(key: "targetPart", defaultValue: "");
		if (!FindCharacter(name: name, target: out var target))
		{
			Log.Print($"找不到名为{name}的目标");
			rootNode.McpCheckPoint();
            return;
		}
		if (!GetBodyPart(actor: actor, partName: attackPart, bodyPartData: out _))
		{
			Log.Print($"无效的攻击部位:{attackPart}");
			rootNode.McpCheckPoint();
            return;
		}
		if (!GetBodyPart(actor: target, partName: targetPart, bodyPartData: out var defenderPart))
		{
			Log.Print($"无效的目标部位:{targetPart}");
			rootNode.McpCheckPoint();
            return;
		}
		actor.actionPoint -= 3;
		var roll = Random.Shared.NextSingle();
		var isHit = roll < DefaultHitChance;
		var rollPercent = (int)(roll * 100);
		var requiredPercent = (int)(DefaultHitChance * 100);
		string message;
		if (isHit)
		{
			defenderPart.hp -= 2;
			message = $"{actor.name}用{attackPart}攻击{target.name}的{targetPart}，命中！({rollPercent}/{requiredPercent})\n造成2点伤害，{target.name}的{targetPart}剩余生命值:{defenderPart.hp}";
		}
		else
		{
			message = $"{actor.name}用{attackPart}攻击{target.name}的{targetPart}，未命中 ({rollPercent}/{requiredPercent})";
		}
		Log.Print(message);
		if (target.Dead) Log.Print($"{target.name}已死亡");
		await rootNode.ShowDialogue(message);
		_ = new TurnProgressState(combatState: combatState, combatData: combatState.combatData);
        return;
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
