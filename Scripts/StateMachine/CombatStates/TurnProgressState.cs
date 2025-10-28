using System;
using System.Collections.Generic;
using System.Linq;
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
		combatData.tickTimer += dt;
		if (combatData.tickTimer >= 2.0)
		{
			combatData.tickTimer -= 2.0;
			combatData.tickCount++;
			Log.Print("过去了1tick");
		}
		if (combatData.characters.Where(c => c.team == 0).All(c => c.Dead))
		{
			Log.Print("玩家全灭，战斗失败");
			_ = new PrepareState(combatState.gameState);
			rootNode.McpCheckPoint();
			return;
		}
		if (combatData.characters.Where(c => c.team == 1).All(c => c.Dead))
		{
			Log.Print("敌人全灭，战斗胜利");
			_ = new PrepareState(combatState.gameState);
			rootNode.McpCheckPoint();
			return;
		}
		foreach (var character in combatData.characters)
		{
			if (character.Dead) continue;
			character.actionPoint += dt;
			if (character.actionPoint >= 0)
			{
				Log.Print($"{character.name}的回合!");
				_ = new ActionState(combatState: combatState, combatData: combatData, actor: character);
				return;
			}
		}
	}
	public override string GetStatus() => "回合进行中";
	private protected override void OnExit() { }
}
