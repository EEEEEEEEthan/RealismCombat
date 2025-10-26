using System;
using System.Collections.Generic;
using RealismCombat.Commands;
namespace RealismCombat.StateMachine;
/// <summary>
///     角色行动状态
/// </summary>
class CharacterActionState(Combat combat, Character character) : State(root: combat.programRoot, owner: combat)
{
	public readonly Character character = character;
	private protected override void OnExit() => throw new NotImplementedException();
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>();
	private protected override string GetStatus() =>
		$"""
		{character.name}的回合
		""";
}
