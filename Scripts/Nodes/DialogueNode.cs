using System;
using Godot;
namespace RealismCombat.Nodes;
using Dialogue = DialogueNode;
partial class DialogueNode : Control
{
	public static Dialogue Show(string label, params (string, Action)[] options)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.dialogueScene).Instantiate<DialogueNode>();
		instance.SetDialogue(text: label, options: options);
		return instance;
	}
	[Export] Label label = null!;
	[Export] Container options = null!;
	[Export] float typewriterSpeed = 0.05f;
	string fullText = "";
	int currentCharIndex;
	float typewriterTimer;
	(string, Action)[]? optionData;
	bool isTyping = true;
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
		this.optionData = options;
		label.Text = "";
		label.VisibleCharacters = 0;
	}
	void ShowOptions()
	{
		if (optionData == null) return;
		for (var i = 0; i < optionData.Length; i++)
		{
			var index = i;
			var (optionText, action) = optionData[index];
			var button = new ButtonNode(text: optionText,
				onClick: () =>
				{
					action();
					QueueFree();
				});
			if (i == 0) button.CallDeferred("grab_focus");
			this.options.AddChild(button);
		}
	}
}
