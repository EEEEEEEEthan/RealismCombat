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
	void SetDialogue(string text, Action<int> callback, string[] optionTexts)
	{
		label.Text = text;
		for (var i = 0; i < optionTexts.Length; i++)
		{
			var index = i;
			var button = new ButtonNode(text: optionTexts[index],
				onClick: () =>
				{
					callback(index);
					QueueFree();
				});
			if (i == 0) button.GrabFocus();
			options.AddChild(button);
		}
	}
}
