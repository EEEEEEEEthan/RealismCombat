using System;
using System.Collections.Generic;
using System.Linq;
using RealismCombat.Commands;
using RealismCombat.Commands.CombatCommands;
using RealismCombat.Data;
using RealismCombat.Nodes;
using RealismCombat.StateMachine.GameStates;
namespace RealismCombat.StateMachine.CombatStates;
class ActionState(CombatState combatState, CombatData combatData, CharacterData actor) : CombatChildState(combatState: combatState, combatData: combatData)
{
	const string key = "ActionCommand";
	public readonly CharacterData actor = actor;
	DialogueNode? dialogueNode;
	bool executing;
	public override string Name => "进行行动";
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>
		{
			{ AttackCommand.name, arguments => new AttackCommand(actionState: this, arguments: arguments) },
		};
	public override void Update(double dt)
	{
		if (combatData.tempData.TryGetValue(key: key, value: out var command))
		{
			if (executing) return; // 指令正在执行(播放执行动画)
			Execute(command); // 读档
		}
		if (actor.PlayerControlled)
		{
			if (dialogueNode is null) Human();
		}
		else
		{
			AI();
		}
	}
	void Human()
	{
		dialogueNode = rootNode.ShowDialogue(text: "请选择行动指令", callback: onSelected, options: ["攻击",]);
		return;
		void onSelected(int index)
		{
			if (index == 0)
			{
				var enemies = combatData.characters.Where(c => c.team != actor.team && !c.Dead).ToList();
				var target = enemies[Random.Shared.Next(enemies.Count)];
				var bodyParts = new[] { "head", "chest", "leftArm", "rightArm", "leftLeg", "rightLeg", };
				var attackPart = bodyParts[Random.Shared.Next(bodyParts.Length)];
				var targetPart = bodyParts[Random.Shared.Next(bodyParts.Length)];
				var command = $"{AttackCommand.name} target {target.name} attackerPart {attackPart} targetPart {targetPart}";
				Execute(command);
			}
		}
	}
	void AI()
	{
		var enemies = combatData.characters.Where(c => c.team != actor.team && !c.Dead).ToList();
		var target = enemies[Random.Shared.Next(enemies.Count)];
		var bodyParts = new[] { "head", "chest", "leftArm", "rightArm", "leftLeg", "rightLeg", };
		var attackPart = bodyParts[Random.Shared.Next(bodyParts.Length)];
		var targetPart = bodyParts[Random.Shared.Next(bodyParts.Length)];
		var command = $"{AttackCommand.name} target {target.name} attackerPart {attackPart} targetPart {targetPart}";
		Execute(command);
	}
	void Execute(string command)
	{
		if (executing) throw new("指令正在执行");
		combatData.tempData[key] = command;
		combatState.gameState.Save();
		ExecuteCommand(command);
		executing = true;
		combatData.tempData.Remove(key);
	}
	private protected override void OnExit()
	{
		if (dialogueNode is not null)
		{
			dialogueNode.QueueFree();
			dialogueNode = null;
		}
	}
	private protected override string GetStatus() => "行动中";
}
