using System;
using Godot;
namespace RealismCombat.Nodes;
partial class DialogueNode : Control
{
	public static DialogueNode ShowDialogue(string text, Action<int> callback, params string[] options)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.dialogueScene).Instantiate<DialogueNode>();
		instance.SetDialogue(text: text, callback: callback, optionTexts: options);
		return instance;
	}
	[Export] Label label = null!;
	[Export] Container options = null!;
	[Export] float typewriterSpeed = 0.05f;
	string fullText = "";
	int currentCharIndex;
	float typewriterTimer;
	Action<int>? callback;
	string[]? optionTexts;
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
	void SetDialogue(string text, Action<int> callback, string[] optionTexts)
	{
		fullText = text;
		this.callback = callback;
		this.optionTexts = optionTexts;
		label.Text = "";
		label.VisibleCharacters = 0;
	}
	void ShowOptions()
	{
		if (optionTexts == null || callback == null) return;
		for (var i = 0; i < optionTexts.Length; i++)
		{
			var index = i;
			var button = new ButtonNode(text: optionTexts[index],
				onClick: () =>
				{
					callback(index);
					QueueFree();
				});
			if (i == 0) button.CallDeferred("grab_focus");
			options.AddChild(button);
		}
	}
}
