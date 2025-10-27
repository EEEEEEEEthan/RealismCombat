using System;
using Godot;
namespace RealismCombat.Nodes;
partial class DialogueNode : HBoxContainer
{
	[Obsolete]
	public static DialogueNode Show(string label, params (string, Action)[] options)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.dialogueScene).Instantiate<DialogueNode>();
		instance.label.Text = label;
		foreach ((var text, var callback) in options) instance.AddOption(option: text, onClick: callback);
		instance.options.Visible = false;
		instance.label.VisibleCharacters = 0;
		return instance;
	}
	public static DialogueNode Create(string label)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.dialogueScene).Instantiate<DialogueNode>();
		instance.label.Text = label;
		instance.label.VisibleCharacters = 0;
		return instance;
	}
	[Export] public Container childrenContainer = null!;
	[Export] Label label = null!;
	[Export] Container options = null!;
	[Export] float typewriterSpeed = 0.05f;
	int currentCharIndex;
	float typewriterTimer;
	bool isTyping = true;
	public void AddOption(string option, Action onClick)
	{
		var button = new ButtonNode(text: option, onClick: onClick);
		options.AddChild(button);
		if (options.GetChildCount() == 1) button.CallDeferred(Control.MethodName.GrabFocus);
	}
	public DialogueNode CreateChild(string label)
	{
		var child = Create(label: label);
		childrenContainer.AddChild(child);
		return child;
	}
	public override void _Process(double delta)
	{
		if (!isTyping) return;
		typewriterTimer += (float)delta;
		if (typewriterTimer >= typewriterSpeed)
		{
			typewriterTimer = 0f;
			label.VisibleCharacters++;
			if (label.VisibleCharacters >= label.Text.Length)
			{
				isTyping = false;
				options.Visible = true;
			}
		}
	}
}
