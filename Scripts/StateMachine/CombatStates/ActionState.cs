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
	internal static readonly string[] bodyParts = Enum.GetNames(typeof(BodyPartCode));
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
			foreach (var part in c.BodyParts) Log.Print($"  {part.id}({part.hp}/{part.maxHp})");
		}
		var targetName = "";
		dialogueNode = rootNode.CreateDialogue("请选择行动指令");
		dialogueNode.AddOption(option: "攻击",
			onClick: () =>
			{
				var dialogSelectBodyPart = dialogueNode.CreateChild("用什么部位发动攻击");
				foreach (var bodyPart in actor.BodyParts)
				{
					var selectedAttackerBodyPart = bodyPart;
					dialogSelectBodyPart.AddOption(option: selectedAttackerBodyPart.id.ToString(),
						onClick: () =>
						{
							var dialogSelectDefender = dialogSelectBodyPart.CreateChild("攻击谁");
							foreach (var c in combatData.characters)
								if (c.team != actor.team && !c.Dead)
								{
									var selectedDefender = c;
									dialogSelectDefender.AddOption(option: selectedDefender.name,
										onClick: () =>
										{
											var dialogSelectBodyPart = dialogSelectDefender.CreateChild($"攻击{selectedDefender.name}的什么部位");
											foreach (var bodyPart in selectedDefender.BodyParts)
											{
												var selectedDefenderBodyPart = bodyPart;
												dialogSelectBodyPart.AddOption(option: selectedDefenderBodyPart.id.ToString(),
													onClick: () =>
													{
														targetName = selectedDefender.name;
														var command =
															$"{AttackCommand.name} target {targetName} attackerPart {selectedAttackerBodyPart.id} targetPart {selectedDefenderBodyPart.id}";
														Execute(command);
														dialogueNode.QueueFree();
													});
											}
											dialogSelectBodyPart.AddOption(option: "返回", onClick: () => { dialogSelectBodyPart.QueueFree(); });
										});
								}
							dialogSelectDefender.AddOption(option: "返回", onClick: () => { dialogSelectDefender.QueueFree(); });
						});
				}
				dialogSelectBodyPart.AddOption(option: "返回", onClick: () => { dialogSelectBodyPart.QueueFree(); });
			});
		rootNode.McpCheckPoint();
	}
	void AI()
	{
		var enemies = combatData.characters.Where(c => c.team != actor.team && !c.Dead).ToList();
		var target = enemies[Random.Shared.Next(enemies.Count)];
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
