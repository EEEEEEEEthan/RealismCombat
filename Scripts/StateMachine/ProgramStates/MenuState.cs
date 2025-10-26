using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Commands.ProgramCommands;
using RealismCombat.Nodes;
namespace RealismCombat.StateMachine.ProgramStates;
class MenuState : State
{
	readonly MainMenuNode menuNode;
	public MenuState(ProgramRootNode rootNode) : base(rootNode: rootNode, owner: rootNode)
	{
		menuNode = MainMenuNode.Create(this);
		rootNode.AddChild(menuNode);
	}
	private protected override void OnExit() => menuNode.QueueFree();
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>
		{
			[StartNewGameCommand.name] = _ => new StartNewGameCommand(rootNode),
		};
	private protected override string GetStatus() => "主菜单";
}
