using Godot;
namespace RealismCombat.Nodes.Components;
partial class PrinterLabelNode : RichTextLabel
{
	const float interval = 0.1f;
	double time;
	public override void _Ready() => VisibleCharacters = 0;
	public override void _Process(double delta)
	{
		time += delta;
		if (time < interval) return;
		time -= interval;
		VisibleCharacters += 1;
	}
	public void Show(string text)
	{
		VisibleCharacters = 0;
		Text = text;
	}
}
