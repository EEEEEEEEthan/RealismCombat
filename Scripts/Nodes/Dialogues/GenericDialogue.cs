using Godot;
using RealismCombat.Nodes.Components;
namespace RealismCombat.Nodes.Dialogues;
partial class GenericDialogue : Control
{
	[Export] PrinterLabelNode printer = null!;
	[Export] TextureRect triangle = null!;
	[Export(hint: PropertyHint.Range, hintString: "0,5")]
	float autoContinue;
	double continueTimer;
	bool anyKeyToContinue;
	double time;
	public override void _Ready() => GrabFocus();
	public override void _Input(InputEvent @event)
	{
		if (anyKeyToContinue && HasFocus() && @event.IsPressed() && !@event.IsEcho()) QueueFree();
	}
	public override void _Process(double delta)
	{
		time += delta;
		if (printer.Printing)
		{
			triangle.Visible = true;
			triangle.Position = Vector2.Down * ((int)time % 2);
			continueTimer = 0;
			if (HasFocus())
			{
				if (Input.IsAnythingPressed())
				{
					printer.speedUp = true;
					triangle.Position = Vector2.Down;
					triangle.SelfModulate = GameColors.activeControl;
				}
				else
				{
					printer.speedUp = false;
					triangle.SelfModulate = GameColors.normalControl;
				}
			}
			return;
		}
		if (autoContinue > 0)
		{
			triangle.Visible = false;
			continueTimer += delta;
			if (continueTimer > autoContinue) QueueFree();
			return;
		}
		triangle.Visible = true;
		triangle.SelfModulate = GameColors.normalControl;
		triangle.Position = default;
		anyKeyToContinue = true;
	}
}
