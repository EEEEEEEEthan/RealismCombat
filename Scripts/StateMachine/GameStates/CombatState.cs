using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Data;
using RealismCombat.Nodes;
using RealismCombat.StateMachine.CombatStates;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.StateMachine.GameStates;
class CombatState : State, IStateOwner
{
	readonly CombatData combatData;
	readonly GameState gameState;
	public override string Name => "战斗";
	public CombatNode CombatNode { get; }
	public State State { get; private set; }
	State IStateOwner.State
	{
		get => State;
		set => State = value;
	}
	public CombatState(GameState gameState, CombatData combatData) : base(rootNode: gameState.rootNode, owner: gameState)
	{
		this.gameState = gameState;
		gameState.gameData.combatData = this.combatData = combatData;
		CombatNode = CombatNode.Create(this);
		gameState.gameNode.AddChild(CombatNode);
		State = new TurnProgressState(this);
		foreach (var character in combatData.characters) CombatNode.AddCharacter(character);
	}
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>(State.GetCommandGetters());
	public override void Update(double dt) => State.Update(dt);
	private protected override void OnExit() => CombatNode.QueueFree();
	private protected override string GetStatus() => "战斗中";
}
