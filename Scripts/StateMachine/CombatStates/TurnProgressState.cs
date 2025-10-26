using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Data;
using RealismCombat.StateMachine.GameStates;
namespace RealismCombat.StateMachine.CombatStates;
class TurnProgressState(CombatState combatState, CombatData combatData) : CombatChildState(combatState: combatState, combatData: combatData)
{
	public override string Name => "回合进行中";
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>();
	public override void Update(double dt)
	{
		foreach (var character in combatData.characters)
		{
			character.actionPoint += dt;
			if (character.actionPoint >= 0)
			{
				_ = new ActionState(combatState: combatState, combatData: combatData, actor: character);
				Log.Print($"{character.name}的回合!");
				if (character.team == 0) rootNode.McpCheckPoint();
				return;
			}
		}
	}
	private protected override void OnExit() { }
	private protected override string GetStatus() => "回合进行中";
}
