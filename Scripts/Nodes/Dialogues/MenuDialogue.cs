using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Godot;
using RealismCombat.Nodes.Components;
namespace RealismCombat.Nodes.Dialogues;
public record DialogueData
{
	public string? title;
	public IReadOnlyList<DialogueOptionData> options = null!;
}
public record DialogueOptionData
{
	public string option = null!;
	public string? description;
	public Action? onPreview;
	public Action onConfirm = null!;
	public bool available;
}
public partial class MenuDialogue : PanelContainer
{
	public static MenuDialogue Create(ProgramRootNode programRoot)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.dialoguesMenudialogue).Instantiate<MenuDialogue>();
		instance.root = programRoot;
		return instance;
	}
	readonly TaskCompletionSource<int> completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
	ProgramRootNode root = null!;
	[Export] Container container = null!;
	[Export] TextureRect arrow = null!;
	[Export] PrinterLabelNode title = null!;
	[Export] PrinterLabelNode description = null!;
	[Export] Control titleControl = null!;
	int index;
	bool completed;
	DialogueData data = null!;
	bool active;
	public bool Active
	{
		get => active;
		set
		{
			if (active == value) return;
			active = value;
			if (active)
			{
				var builder = new StringBuilder();
				builder.AppendLine(data.title?.Trim());
				builder.AppendLine("请做出选择(game_select_option):");
				for (var i = 0; i < data.options.Count; i++)
				{
					var option = data.options[i];
					//if (!option.available) builder.AppendLine($"{i}. (not available) {option.option?.Trim()} {option.description?.Trim()}");
					builder.AppendLine($"{i}. {option.option?.Trim()} {option.description?.Trim()}");
				}
				Log.Print(builder.ToString());
				root.McpRespond();
			}
			UpdateTitleControl();
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (!Active) return;
		if (container.GetChildCount() == 0) return;
		var moveUp = Input.IsActionJustPressed("ui_up");
		var moveDown = Input.IsActionJustPressed("ui_down");
		var accept = Input.IsActionJustPressed("ui_accept");
		if (moveUp)
			Select((index - 1 + container.GetChildCount()) % container.GetChildCount());
		else if (moveDown)
			Select((index + 1) % container.GetChildCount());
		else if (accept) Confirm(index);
	}
	public override void _Process(double delta)
	{
		if (container.GetChildCount() == 0) return;
		arrow.Position = container.GetChild<Control>(index).Position with { X = -6, };
		arrow.SelfModulate = Input.IsAnythingPressed() && Active ? GameColors.activeControl : GameColors.normalControl;
	}
	public override void _ExitTree()
	{
		Complete();
		base._ExitTree();
	}
	public void Confirm(int index)
	{
		Select(index);
		var option = data.options[index];
		if (!option.available) throw new InvalidOperationException("选项不可用，无法确认");
		Log.Print($"选择了{index},{option.option}");
		option.onConfirm();
		Complete();
	}
	public void Select(int index)
	{
		var text = data.options[index].description;
		if (text != null) description.Show(text);
		data.options[index].onPreview?.Invoke();
		this.index = index;
	}
	public TaskAwaiter<int> GetAwaiter() => completionSource.Task.GetAwaiter();
	public void Initialize(DialogueData data, bool active = true)
	{
		this.data = data;
		if (data.options.Count < 1) throw new ArgumentException("至少需要一个选项才能显示菜单对话框");
		if (data.title != null) title.Show(data.title);
		foreach (var optionData in data.options) AddOption(optionData);
		UpdateTitleControl();
		Active = active;
	}
	void UpdateTitleControl() => titleControl.Visible = Active && !string.IsNullOrEmpty(data.title);
	void AddOption(DialogueOptionData data)
	{
		container.AddChild(new Label { Text = data.option, });
		if (container.GetChildCount() == 1)
		{
			var text = this.data.options[0].description;
			if (text != null) description.Show(text);
		}
	}
	void Complete()
	{
		if (completed) return;
		completed = true;
		completionSource.TrySetResult(index);
	}
}
