using System;
using System.Collections.Generic;
using Godot;
using RealismCombat.Extensions;
using RealismCombat.Nodes.Components;
namespace RealismCombat.Nodes.Dialogues;
partial class MenuDialogue : Node
{
	public static MenuDialogue Create()
	{
		var scene = GD.Load<PackedScene>(ResourceTable.dialoguesMenudialogue);
		return scene.Instantiate<MenuDialogue>();
	}
	readonly List<(string desc, Action callback)> options = [];
	[Export] Container container = null!;
	[Export] TextureRect arrow = null!;
	[Export] PrinterLabelNode title = null!;
	[Export] PrinterLabelNode description = null!;
	int index;
	public string Title
	{
		get => title.Text;
		set => title.Show(value);
	}
	public override void _Input(InputEvent @event)
	{
		if (container.GetChildCount() == 0) return;
		var moveUp = Input.IsActionJustPressed("ui_up");
		var moveDown = Input.IsActionJustPressed("ui_down");
		var accept = Input.IsActionJustPressed("ui_accept");
		if (moveUp)
		{
			index = (index - 1 + container.GetChildCount()) % container.GetChildCount();
			description.Show(options[index].desc);
		}
		else if (moveDown)
		{
			index = (index + 1) % container.GetChildCount();
			description.Show(options[index].desc);
		}
		else if (accept)
		{
			options[index].callback();
		}
	}
	public override void _Process(double delta)
	{
		if (container.GetChildCount() == 0) return;
		arrow.Position = container.GetChild<Control>(index).Position with { X = -6, };
		arrow.SelfModulate = Input.IsAnythingPressed() ? GameColors.activeControl : GameColors.normalControl;
	}
	public void AddOption(string option, string description, Action callback)
	{
		container.AddChild(new Label { Text = option, });
		options.Add((description, callback));
	}
	public void ClearOptions() => container.DestroyChildren();
}
