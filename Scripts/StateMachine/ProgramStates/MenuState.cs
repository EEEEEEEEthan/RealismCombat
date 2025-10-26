using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Commands.ProgramCommands;
using RealismCombat.Nodes;
namespace RealismCombat.StateMachine.ProgramStates;
class MenuState : State
{
	readonly MainMenu menu;
	public MenuState(ProgramRoot root) : base(root: root, owner: root)
	{
		menu = MainMenu.Create(this);
		root.AddChild(menu);
	}
	private protected override void OnExit() => menu.QueueFree();
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>
		{
			[StartNewGameCommand.name] = _ => new StartNewGameCommand(root),
		};
	private protected override string GetStatus() => "主菜单";
}
