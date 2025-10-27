using System;
using System.Collections.Generic;
using System.Linq;
using RealismCombat.Commands;
using RealismCombat.Commands.CombatCommands;
using RealismCombat.Data;
using RealismCombat.Extensions;
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
		if (executing) return;
		if (combatData.tempData.TryGetValue(key: key, value: out var command))
		{
			Execute(command); // 读档
			return;
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
	public override string GetStatus() => "行动中";
	void Human()
	{
		foreach (var c in combatData.characters)
		{
			Log.Print($"{c.name}(team={c.team}):");
			foreach (var part in c.BodyParts) Log.Print($"  {part.bodyPart}({part.hp}/{part.maxHp})");
		}
		var target = "";
		dialogueNode = rootNode.ShowDialogue(text: "请选择行动指令",
			options: ("攻击", () =>
			{
				if (dialogueNode is null) throw new("dialogueNode为空");
				DialogueNode child = null!;
				var options = new List<(string, Action)>();
				foreach (var c in combatData.characters)
					if (c.team != actor.team && !c.Dead)
					{
						var copied = c;
						options.Add((copied.name, () =>
						{
							target = copied.name;
							var bodyParts = new[] { "head", "chest", "leftArm", "rightArm", "leftLeg", "rightLeg", };
							var attackPart = bodyParts[Random.Shared.Next(bodyParts.Length)];
							var targetPart = bodyParts[Random.Shared.Next(bodyParts.Length)];
							var command = $"{AttackCommand.name} target {target} attackerPart {attackPart} targetPart {targetPart}";
							Execute(command);
							dialogueNode.QueueFree();
						}));
					}
				options.Add(("返回", () => { child.QueueFree(); }));
				child = dialogueNode.ShowChild(label: "攻击目标", options: options.ToArray());
			}));
		rootNode.McpCheckPoint();
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
		ExecuteCommandTask(command);
		executing = true;
		combatData.tempData.Remove(key);
	}
	private protected override void OnExit()
	{
		if (dialogueNode != null && dialogueNode.Valid())
		{
			dialogueNode.QueueFree();
			dialogueNode = null;
		}
	}
}
