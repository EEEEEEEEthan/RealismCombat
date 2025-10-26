using System;
using System.Collections.Generic;
using System.Linq;
using RealismCombat.Commands;
using RealismCombat.Commands.CombatCommands;
using RealismCombat.Data;
using RealismCombat.StateMachine.GameStates;
namespace RealismCombat.StateMachine.CombatStates;
class ActionState(CombatState combatState, CombatData combatData, CharacterData actor) : CombatChildState(combatState: combatState, combatData: combatData)
{
	public readonly CharacterData actor = actor;
	public override string Name => "进行行动";
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters()
	{
		if (!actor.PlayerControlled) return new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>();
		return new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>
		{
			{ AttackCommand.name, arguments => new AttackCommand(actionState: this, arguments: arguments) },
		};
	}
	public override void Update(double dt)
	{
		if (actor.PlayerControlled) return;
		var enemies = combatData.characters.Where(c => c.team != actor.team && !c.Dead).ToList();
		if (enemies.Count > 0)
		{
			var target = enemies[Random.Shared.Next(enemies.Count)];
			var bodyParts = new[] { "head", "chest", "leftArm", "rightArm", "leftLeg", "rightLeg", };
			var attackPart = bodyParts[Random.Shared.Next(bodyParts.Length)];
			var targetPart = bodyParts[Random.Shared.Next(bodyParts.Length)];
			var attackCommand = new AttackCommand(actionState: this,
				arguments: new Dictionary<string, string>
				{
					{ "target", target.name },
					{ "attackerPart", attackPart },
					{ "targetPart", targetPart },
				});
			attackCommand.Execute();
		}
	}
	private protected override void OnExit() { }
	private protected override string GetStatus() => "行动中";
}
