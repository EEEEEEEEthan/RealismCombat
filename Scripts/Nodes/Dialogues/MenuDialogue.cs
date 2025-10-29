using System;
using System.Collections.Generic;
using Godot;
using RealismCombat.Extensions;
namespace RealismCombat.Nodes.Dialogues;
partial class MenuDialogue : Node
{
	readonly List<Action> callbacks = [];
	[Export] Container container = null!;
	[Export] TextureRect arrow = null!;
	int index;
	public override void _Input(InputEvent @event) { }
	public override void _Ready() { }
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
