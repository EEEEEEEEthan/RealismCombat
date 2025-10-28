using Godot;
using RealismCombat.Nodes.Components;
namespace RealismCombat.Nodes.Dialogues;
partial class GenericDialogue : Control
{
	[Export] PrinterLabelNode printer = null!;
	[Export] TextureRect triangle = null!;
	[Export] bool autoContinue;
	double continueTimer;
	bool anyKeyToContinue;
	public override void _Ready() => GrabFocus();
	public override void _Input(InputEvent @event)
	{
		if (anyKeyToContinue && HasFocus() && @event.IsPressed() && !@event.IsEcho()) QueueFree();
	}
	public override void _Process(double delta)
	{
		if (HasFocus()) printer.speedUp = Input.IsAnythingPressed();
		if (printer.Printing)
		{
			triangle.Visible = true;
			continueTimer = 0;
		}
		else
		{
			if (autoContinue)
			{
				triangle.Visible = false;
				continueTimer += delta;
			}
			else
			{
				triangle.Visible = true;
				anyKeyToContinue = true;
			}
		}
	}
}
