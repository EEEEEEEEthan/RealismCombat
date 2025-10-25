using Godot;
using RealismCombat.Commands;
namespace RealismCombat;
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
	void OnButtonPressed() => root.CommandHandler.Handle(StartNextCombat.name);
}
