using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealismCombat.Data;
using RealismCombat.StateMachine.CombatStates;
namespace RealismCombat.Commands.CombatCommands;
class AttackCommand : CombatCommand
{
	public const string name = "combat_attack";
	/// <summary>
	///     默认命中率（0-1）
	/// </summary>
	const float defaultHitChance = 0.8f;
	static bool GetBodyPart(CharacterData actor, string partName, out BodyPartData bodyPartData)
	{
		bodyPartData = partName switch
		{
			nameof(BodyPartCode.Head) => actor.head,
			nameof(BodyPartCode.Chest) => actor.chest,
			nameof(BodyPartCode.LeftArm) => actor.leftArm,
			nameof(BodyPartCode.RightArm) => actor.rightArm,
			nameof(BodyPartCode.LeftLeg) => actor.leftLeg,
			nameof(BodyPartCode.RightLeg) => actor.rightLeg,
			_ => null!,
		};
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		return bodyPartData != null;
	}
	readonly ActionState actionState;
	public AttackCommand(ActionState actionState, IReadOnlyDictionary<string, string>? arguments = null) :
		base(combatState: actionState.combatState, arguments: arguments) =>
		this.actionState = actionState;
	public AttackCommand(ActionState actionState, string command) : base(combatState: actionState.combatState, command: command) =>
		this.actionState = actionState;
	public override bool Validate(out string error)
	{
		var actor = actionState.actor;
		var name = arguments.GetValueOrDefault(key: "target", defaultValue: "");
		var attackPart = arguments.GetValueOrDefault(key: "attackerPart", defaultValue: "");
		var targetPart = arguments.GetValueOrDefault(key: "targetPart", defaultValue: "");
		if (!FindCharacter(name: name, target: out var target))
		{
			error = $"找不到名为{name}的目标";
			return false;
		}
		if (!GetBodyPart(actor: actor, partName: attackPart, bodyPartData: out _))
		{
			error = $"无效的攻击部位:{attackPart}";
			return false;
		}
		if (!GetBodyPart(actor: target, partName: targetPart, bodyPartData: out _))
		{
			error = $"无效的目标部位:{targetPart}";
			return false;
		}
		error = "";
		return true;
	}
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
		const int actionPoints = 3;
		var roll = Random.Shared.NextSingle();
		var hit = roll < defaultHitChance;
		var rollPercent = (int)(roll * 100);
		const int requiredPercent = (int)(defaultHitChance * 100);
		const int damage = 2;
		string message;
		if (hit)
		{
			var remainingHp = defenderPart.hp - damage;
			message =
				$"{actor.name}用{attackPart}攻击{target.name}的{targetPart}，命中！({rollPercent}/{requiredPercent})\n造成{damage}点伤害，{target.name}的{targetPart}剩余生命值:{remainingHp}";
		}
		else
		{
			message = $"{actor.name}用{attackPart}攻击{target.name}的{targetPart}，未命中 ({rollPercent}/{requiredPercent})";
		}
		await rootNode.ShowDialogue(text: message, timeout: null);
		actor.actionPoint -= actionPoints;
		if (hit) defenderPart.hp -= damage;
		Log.Print(message);
		if (target.Dead) Log.Print($"{target.name}已死亡");
		_ = new TurnProgressState(combatState: combatState, combatData: combatState.combatData);
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
