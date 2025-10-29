using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using RealismCombat.Nodes.Components;
namespace RealismCombat.Nodes.Dialogues;
public struct DialogueData
{
	public string title;
	public DialogueOptionData[] options;
}
public struct DialogueOptionData
{
	public string option;
	public string description;
	public Action onPreview;
	public Action onConfirm;
	public bool available;
}
partial class MenuDialogue : Control
{
	public static MenuDialogue Create(DialogueData data)
	{
		if (data.options.Length < 1) throw new ArgumentException("至少需要一个选项才能创建菜单对话框");
		var instance = GD.Load<PackedScene>(ResourceTable.dialoguesMenudialogue).Instantiate<MenuDialogue>();
		foreach (var optionData in data.options) instance.AddOption(optionData);
		return instance;
	}
	readonly List<DialogueOptionData> options = [];
	readonly TaskCompletionSource<int> completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
	[Export] Container container = null!;
	[Export] TextureRect arrow = null!;
	[Export] PrinterLabelNode title = null!;
	[Export] PrinterLabelNode description = null!;
	int index;
	bool completed;
	public override void _Input(InputEvent @event)
	{
		if (container.GetChildCount() == 0) return;
		var moveUp = Input.IsActionJustPressed("ui_up");
		var moveDown = Input.IsActionJustPressed("ui_down");
		var accept = Input.IsActionJustPressed("ui_accept");
		if (moveUp)
		{
			index = (index - 1 + container.GetChildCount()) % container.GetChildCount();
			description.Show(options[index].description);
		}
		else if (moveDown)
		{
			index = (index + 1) % container.GetChildCount();
			description.Show(options[index].description);
		}
		else if (accept)
		{
			options[index].onConfirm();
			Complete();
		}
	}
	public override void _Process(double delta)
	{
		if (container.GetChildCount() == 0) return;
		arrow.Position = container.GetChild<Control>(index).Position with { X = -6, };
		arrow.SelfModulate = Input.IsAnythingPressed() ? GameColors.activeControl : GameColors.normalControl;
	}
	public override void _ExitTree()
	{
		Complete();
		base._ExitTree();
	}
	public TaskAwaiter<int> GetAwaiter() => completionSource.Task.GetAwaiter();
	void AddOption(DialogueOptionData data)
	{
		container.AddChild(new Label { Text = data.option, });
		options.Add(data);
		if (options.Count == 1) description.Show(options[index].description);
	}
	void Complete()
	{
		if (completed) return;
		completed = true;
		completionSource.TrySetResult(index);
	}
}
