using System;
using System.Threading.Tasks;
using Godot;
namespace RealismCombat.Nodes;
partial class DialogueNode : Control
{
	public Task ShowDialogue(string text, params (string optionText, Action onSelected)[] options)
	{
	}
	[Export] Label label = null!;
	[Export] Container options = null!;
	void SetDialogue(string text, params (string optionText, Action onSelected)[] options)
	{
		label.Text = text;
		foreach ((var txt, var callback) in options) this.options.AddChild(new ButtonNode(text: txt, onClick: callback));
	}
}
