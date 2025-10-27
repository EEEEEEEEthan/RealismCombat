using System;
using Godot;
namespace RealismCombat.Nodes;
[Tool, GlobalClass,]
partial class ButtonNode : Button
{
	readonly Action? onClick;
	public ButtonNode(string text, Action onClick)
	{
		Text = text;
		this.onClick = onClick;
		Pressed += onClick;
	}
	public ButtonNode() => onClick = null;
	public override void _Ready()
	{
		Alignment = HorizontalAlignment.Left;
		FocusEntered += OnFocusEntered;
		FocusExited += OnFocusExited;
		Icon = GD.Load<Texture2D>(ResourceTable.emptyIcon);
	}
	void OnFocusEntered() => Icon = null;
	void OnFocusExited() => Icon = GD.Load<Texture2D>(ResourceTable.emptyIcon);
}
