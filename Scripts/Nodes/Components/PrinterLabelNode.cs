using Godot;
using RealismCombat.Nodes;
namespace RealismCombat.Nodes.Components;
partial class PrinterLabelNode : RichTextLabel
{
	public float interval = 0.1f;
	public bool speedUp;
	double time;
	ProgramRootNode? root;
	public bool Printing => VisibleCharacters < Text.Length;
	public override void _Ready()
	{
		VisibleCharacters = 0;
		root = GetNodeOrNull<ProgramRootNode>("/root/ProgramRoot");
	}
	public override void _Process(double delta)
	{
		var currentInterval = speedUp ? interval * 0.1f : interval;
		time += delta;
		if (time < currentInterval) return;
		time -= currentInterval;
		if (time > 0) time = 0;
		VisibleCharacters += 1;
		if (root != null && Printing)
		{
			root.PlaySoundEffect(AudioTable.retroclick236673);
		}
		if (!Printing) SetProcess(false);
	}
	public void Show(string text)
	{
		SetProcess(true);
		VisibleCharacters = 0;
		Text = text;
	}
}
