using Godot;
using RealismCombat.Game.Commands;
namespace RealismCombat.Game;
public partial class PrepareCanvas : Node
{
	GameRoot root = null!;
	Button buttonNextBattle = null!;
	public override void _Ready()
	{
		root = GetParent<GameRoot>();
		buttonNextBattle = GetNode<Button>("ButtonNextBattle");
		buttonNextBattle.Pressed += OnButtonPressed;
	}
	void OnButtonPressed() => _ = root.CommandHandler.Handle(StartNextCombat.NAME);
}
