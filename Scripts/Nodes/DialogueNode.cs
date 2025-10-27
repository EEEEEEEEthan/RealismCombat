using System;
using Godot;
namespace RealismCombat.Nodes;
partial class DialogueNode : Control
{
	public static DialogueNode Show(string label, params (string, Action)[] options)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.dialogueScene).Instantiate<DialogueNode>();
		instance.SetDialogue(text: label, options: options);
		return instance;
	}
	[Export] public Container childrenContainer = null!;
	[Export] Label label = null!;
	[Export] Container options = null!;
	[Export] float typewriterSpeed = 0.05f;
	string fullText = "";
	int currentCharIndex;
	float typewriterTimer;
	(string, Action)[]? optionData;
	bool isTyping = true;
	public DialogueNode ShowChild(string label, params (string, Action)[] options)
	{
		var child = Show(label: label, options: options);
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
			currentCharIndex++;
			label.VisibleCharacters = currentCharIndex;
			label.Text = fullText[..currentCharIndex];
			if (currentCharIndex >= fullText.Length)
			{
				isTyping = false;
				ShowOptions();
			}
		}
	}
	void SetDialogue(string text, (string, Action)[] options)
	{
		fullText = text;
		optionData = options;
		label.Text = "";
		label.VisibleCharacters = 0;
	}
	void ShowOptions()
	{
		if (optionData == null) return;
		for (var i = 0; i < optionData.Length; i++)
		{
			var index = i;
			(var optionText, var action) = optionData[index];
			var button = new ButtonNode(text: optionText,
				onClick: () => { action(); });
			if (i == 0) button.CallDeferred("grab_focus");
			options.AddChild(button);
		}
	}
}
