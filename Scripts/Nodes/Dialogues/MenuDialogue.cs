using System;
using System.Collections.Generic;
using Godot;
using RealismCombat.Extensions;
namespace RealismCombat.Nodes.Dialogues;
partial class MenuDialogue : Node
{
	public static MenuDialogue Create()
	{
		var scene = GD.Load<PackedScene>(ResourceTable.dialoguesMenudialogue);
		return scene.Instantiate<MenuDialogue>();
	}
	readonly List<Action> callbacks = [];
	[Export] Container container = null!;
	[Export] TextureRect arrow = null!;
	int index;
	public override void _Input(InputEvent @event)
	{
		if (container.GetChildCount() == 0) return;
		var moveUp = Input.IsActionJustPressed("ui_up");
		var moveDown = Input.IsActionJustPressed("ui_down");
		var accept = Input.IsActionJustPressed("ui_accept");
		if (moveUp)
			index = (index - 1 + container.GetChildCount()) % container.GetChildCount();
		else if (moveDown)
			index = (index + 1) % container.GetChildCount();
		else if (accept) callbacks[index]();
	}
	public override void _Process(double delta)
	{
		if (container.GetChildCount() == 0) return;
		arrow.Position = container.GetChild<Control>(index).Position with { X = -6, };
		arrow.SelfModulate = Input.IsAnythingPressed() ? GameColors.activeControl : GameColors.normalControl;
	}
	public void AddOption(string option, Action callback)
	{
		container.AddChild(new Label { Text = option, });
		callbacks.Add(callback);
	}
	public void ClearOptions() => container.DestroyChildren();
}
