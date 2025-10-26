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
	readonly GameState gameState;
	readonly CombatData combatData;
	readonly CombatNode combatNode;
	public State State { get; private set; }
	State IStateOwner.State
	{
		get => State;
		set => State = value;
	}
	public CombatState(GameState gameState) : base(rootNode: gameState.rootNode, owner: gameState)
	{
		this.gameState = gameState;
		combatData = new();
		combatNode = CombatNode.Create(this);
		gameState.gameNode.AddChild(combatNode);
		State = new TurnProgressState(this);
	}
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>(State.GetCommandGetters());
	public override void Update(double dt) => State.Update(dt);
	private protected override void OnExit() => combatNode.QueueFree();
	private protected override string GetStatus() => "战斗中";
}
