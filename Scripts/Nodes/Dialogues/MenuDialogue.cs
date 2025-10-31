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
public partial class MenuDialogue : Control
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
	[Export] Label title = null!;
	[Export] PrinterLabelNode description = null!;
	[Export] Control titleControl = null!;
	int index;
	bool completed;
	DialogueData data = null!;
	bool active;
	Tween? shakeTween;
	Vector2? titleControlOriginalPosition;
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
				builder.Append(data.title?.Trim() + "\n");
				builder.Append("请做出选择(game_select_option):\n");
				for (var i = 0; i < data.options.Count; i++)
				{
					var option = data.options[i];
					builder.Append($"{i}. {option.option?.Trim()} {option.description?.Trim()}\n");
				}
				Modulate = Colors.White;
				Log.Print(builder.ToString());
				root.McpRespond();
			}
			else
			{
				Modulate = GameColors.inactiveControl;
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
		if (!option.available)
		{
			Log.Print($"选项不可用:{option.description}");
			ShakeTitleControl();
			root.McpRespond();
			return;
		}
		Log.Print($"选择了{index},{option.option}");
		root.PlaySoundEffect(AudioTable.gameboypluck41265);
		option.onConfirm();
		Complete();
	}
	public void Select(int index)
	{
		if (this.index != index) root.PlaySoundEffect(AudioTable.oneBeep99630);
		var text = data.options[index].description;
		if (text != null) description.Show(text);
		data.options[index].onPreview?.Invoke();
		this.index = index;
	}
	public TaskAwaiter<int> GetAwaiter() => completionSource.Task.GetAwaiter();
	public void Initialize(DialogueData data, bool active = true, Action? onReturn = null, string? returnDescription = null)
	{
		this.data = data;
		if (onReturn != null)
		{
			var returnOption = new DialogueOptionData
			{
				option = "返回",
				description = returnDescription,
				onPreview = () => { },
				onConfirm = () =>
				{
					QueueFree();
					onReturn();
				},
				available = true,
			};
			var optionsList = new List<DialogueOptionData>(data.options) { returnOption, };
			this.data = data with { options = optionsList, };
		}
		if (this.data.options.Count < 1) throw new ArgumentException("至少需要一个选项才能显示菜单对话框");
		if (this.data.title != null) title.Text = this.data.title;
		foreach (var optionData in this.data.options) AddOption(optionData);
		UpdateTitleControl();
		Active = active;
	}
	void ShakeTitleControl()
	{
		if (shakeTween != null)
		{
			shakeTween.Kill();
			if (titleControlOriginalPosition.HasValue) titleControl.Position = titleControlOriginalPosition.Value;
		}
		titleControlOriginalPosition ??= titleControl.Position;
		var originalPosition = titleControlOriginalPosition.Value;
		shakeTween = CreateTween();
		const float shakeDistance = 8.0f;
		const float shakeDuration = 0.1f;
		shakeTween.TweenProperty(@object: titleControl, property: "position:x", finalVal: originalPosition.X + shakeDistance, duration: shakeDuration);
		shakeTween.TweenProperty(@object: titleControl, property: "position:x", finalVal: originalPosition.X - shakeDistance, duration: shakeDuration);
		shakeTween.TweenProperty(@object: titleControl, property: "position:x", finalVal: originalPosition.X + shakeDistance, duration: shakeDuration);
		shakeTween.TweenProperty(@object: titleControl, property: "position:x", finalVal: originalPosition.X, duration: shakeDuration);
		shakeTween.Finished += () =>
		{
			titleControl.Position = originalPosition;
			shakeTween = null;
		};
	}
	void UpdateTitleControl() => titleControl.Visible = Active && !string.IsNullOrEmpty(data.title);
	void AddOption(DialogueOptionData data)
	{
		var label = new Label { Text = data.option, };
		if (!data.available) label.Modulate = GameColors.inactiveControl;
		container.AddChild(label);
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
