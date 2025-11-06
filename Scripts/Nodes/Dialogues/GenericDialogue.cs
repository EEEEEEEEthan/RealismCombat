using System.Collections.Generic;
using Godot;
namespace RealismCombat.Nodes.Dialogues;
[Tool, GlobalClass,]
public partial class GenericDialogue : BaseDialogue
{
	readonly List<string> texts = [];
	int currentTextIndex = -1;
	Printer printer;
	TextureRect icon;
	VBoxContainer container;
	double time;
	public GenericDialogue()
	{
		container = new();
		container.Name = "VBoxContainer";
		AddChild(container);
		{
			printer = new();
			container.AddChild(printer);
			printer.SizeFlagsVertical = SizeFlags.ExpandFill;
		}
		{
			icon = new();
			icon.Name = "Icon";
			container.AddChild(icon);
			icon.Texture = SpriteTable.arrowDown;
			icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		}
		CustomMinimumSize = new(128, 96);
		SetAnchorsPreset(LayoutPreset.BottomWide);
		SetOffset(Side.Left, 0);
		SetOffset(Side.Right, 0);
		SetOffset(Side.Bottom, 0);
		SetOffset(Side.Top, -CustomMinimumSize.Y);
	}
	public void SetTexts(IEnumerable<string> newTexts)
	{
		texts.Clear();
		texts.AddRange(newTexts);
		printer.VisibleCharacters = 0;
		if (texts.Count < 0)
		{
			currentTextIndex = -1;
		}
		else
		{
			currentTextIndex = 0;
			printer.Text = texts[0];
		}
	}
	public void SetText(string text) => SetTexts([text,]);
	public override void _Process(double delta)
	{
		if (Input.IsAnythingPressed())
			printer.interval = 0;
		else
			printer.interval = 0.1f;
		if (printer.Printing || string.IsNullOrEmpty(printer.Text))
		{
			icon.SelfModulate = Colors.Transparent;
		}
		else
		{
			time += delta;
			icon.SelfModulate = time > 0.5 ? Colors.White : Colors.Transparent;
			if (time > 1) time = 0;
		}
	}
	protected override void HandleInput(InputEvent @event)
	{
		if (@event.IsPressed() && !@event.IsEcho() && !printer.Printing)
		{
			currentTextIndex++;
			if (currentTextIndex < texts.Count) printer.Text += "\n" + texts[currentTextIndex];
		}
	}
}
