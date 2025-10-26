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
	State state;
	public override string Name => "战斗";
	public CombatNode CombatNode { get; }
	public State State
	{
		get => state;
		private set
		{
			state = value;
			combatData.state = value switch
			{
				TurnProgressState => 0,
				ActionState => 1,
			};
		}
	}
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
		state = combatData.state switch
		{
			0 => new TurnProgressState(this),
			1 => new ActionState(this),
			_ => throw new($"unexpected state id: {combatData.state}"),
		};
		foreach (var character in combatData.characters) CombatNode.AddCharacter(character);
	}
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>(State.GetCommandGetters());
	public override void Update(double dt) => State.Update(dt);
	private protected override void OnExit() => CombatNode.QueueFree();
	private protected override string GetStatus() => "战斗中";
}
