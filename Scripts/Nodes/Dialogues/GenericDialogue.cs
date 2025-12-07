using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
[Tool, GlobalClass,]
public partial class GenericDialogue : BaseDialogue
{
	static bool InMcpMode => LaunchArgs.port != null;
	readonly List<(TextureRect pointer, Label label)> optionEntries = [];
	readonly Printer printer;
	readonly TextureRect icon;
	readonly VBoxContainer container;
	readonly HBoxContainer optionsContainer;
	TaskCompletionSource<int>? task;
	int selectedOptionIndex = -1;
	double time;
	bool keyDown;
	bool mcpAutomationProcessed;
	public GenericDialogue()
	{
		container = new();
		container.Name = "VBoxContainer";
		AddChild(container);
		printer = new();
		container.AddChild(printer);
		printer.SizeFlagsVertical = SizeFlags.ExpandFill;
		icon = new();
		icon.Name = "Icon";
		container.AddChild(icon);
		icon.Texture = SpriteTable.arrowDown;
		icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		optionsContainer = new()
		{
			Name = "OptionsContainer",
			Alignment = BoxContainer.AlignmentMode.Begin,
			Visible = false,
		};
		optionsContainer.SizeFlagsVertical = SizeFlags.ShrinkBegin;
		optionsContainer.AddThemeConstantOverride("separation", 8);
		container.AddChild(optionsContainer);
		printer.VisibleCharacters = 0;
		printer.Text = string.Empty;
		printer.AutowrapMode = TextServer.AutowrapMode.Arbitrary;
		printer.FitContent = true;
	}
	public override void _Process(double delta)
	{
		if (keyDown && !Input.IsAnythingPressed()) keyDown = false;
		printer.interval = keyDown ? 0 : 0.1f;
		var printing = printer.Printing;
		if (task is not null && !printing && optionEntries.Count > 0 && !optionsContainer.Visible)
		{
			optionsContainer.Visible = true;
			UpdateIconVisibility();
		}
		var hasOptions = optionsContainer.Visible && optionEntries.Count > 0;
		if (task is null || printing || string.IsNullOrEmpty(printer.Text) || hasOptions)
		{
			icon.SelfModulate = GameColors.transparent;
		}
		else
		{
			time += delta;
			icon.SelfModulate = time > 0.5 ? new(1, 1, 1) : GameColors.transparent;
			if (time > 1) time = 0;
		}
		if (InMcpMode)
		{
			if (mcpAutomationProcessed) return;
			if (task is null || printing) return;
			mcpAutomationProcessed = true;
			if (optionEntries.Count > 0)
			{
				GameServer.McpCheckpoint();
				return;
			}
			CompleteActiveTask(-1);
		}
	}
	/// <summary>
	///     追加文本并在完成打印或选择后返回
	/// </summary>
	/// <param name="text">要显示的文本</param>
	/// <param name="options">可选的选项内容</param>
	/// <returns>没有选项返回 -1，有选项返回所选索引</returns>
	public Task<int> ShowTextTask(string text, params string[] options)
	{
		if (task is { Task.IsCompleted: false, }) throw new InvalidOperationException("当前文本尚未完成");
		mcpAutomationProcessed = false;
		task = new();
		time = 0;
		keyDown = false;
		ClearOptions();
		var content = string.IsNullOrEmpty(text) ? string.Empty : text;
		var prefix = string.IsNullOrEmpty(printer.Text) ? string.Empty : "\n";
		var previousCharacters = printer.GetTotalCharacterCount();
		printer.Text += prefix + content;
		printer.VisibleCharacters = previousCharacters;
		Log.Print(content);
		if (options is not { Length: > 0, })
		{
			printer.SizeFlagsVertical = SizeFlags.ExpandFill;
		}
		else
		{
			printer.SizeFlagsVertical = SizeFlags.Expand;
			ClearOptions();
			Log.Print("请选择(game_select_option)");
			optionsContainer.Visible = !printer.Printing;
			optionsContainer.SizeFlagsVertical = SizeFlags.ShrinkBegin;
			for (var i = 0; i < options.Length; i++)
			{
				var optionBox = new VBoxContainer
				{
					Name = $"Option{i}",
					Alignment = BoxContainer.AlignmentMode.Begin,
				};
				optionBox.SizeFlagsVertical = SizeFlags.ShrinkBegin;
				var pointer = new TextureRect
				{
					Texture = SpriteTable.arrowDown,
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
					Visible = false,
				};
				var label = new Label
				{
					Text = options[i],
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top,
				};
				optionBox.AddChild(pointer);
				optionBox.AddChild(label);
				optionsContainer.AddChild(optionBox);
				optionEntries.Add((pointer, label));
				Log.Print($"{i} - {options[i]}");
			}
			if (optionEntries.Count > 0) SelectOption(0);
		}
		UpdateIconVisibility();
		return task.Task;
	}
	public void SelectAndConfirm(int index)
	{
		if (task is null) return;
		if (optionEntries.Count == 0)
		{
			CompleteActiveTask(-1);
			return;
		}
		SelectOption(index);
		ConfirmSelection();
	}
	protected override void HandleInput(InputEvent @event)
	{
		if (!@event.IsPressed() || @event.IsEcho()) return;
		if (task is null) return;
		if (printer.Printing)
		{
			keyDown = true;
			return;
		}
		if (optionEntries.Count == 0)
		{
			CompleteActiveTask(-1);
			return;
		}
		if (@event.IsActionPressed("ui_accept"))
		{
			GetViewport().SetInputAsHandled();
			ConfirmSelection();
		}
		else if (@event.IsActionPressed("ui_left") || @event.IsActionPressed("ui_up"))
		{
			GetViewport().SetInputAsHandled();
			MoveSelection(-1);
		}
		else if (@event.IsActionPressed("ui_right") || @event.IsActionPressed("ui_down"))
		{
			GetViewport().SetInputAsHandled();
			MoveSelection(1);
		}
	}
	void ClearOptions()
	{
		foreach (var child in optionsContainer.GetChildren()) child.QueueFree();
		optionEntries.Clear();
		optionsContainer.Visible = false;
		selectedOptionIndex = -1;
	}
	void SelectOption(int index)
	{
		if (index < 0 || index >= optionEntries.Count) return;
		if (selectedOptionIndex == index && optionEntries[index].pointer.Visible) return;
		for (var i = 0; i < optionEntries.Count; i++) optionEntries[i].pointer.Visible = i == index;
		selectedOptionIndex = index;
	}
	void MoveSelection(int delta)
	{
		if (optionEntries.Count == 0) return;
		if (selectedOptionIndex < 0) selectedOptionIndex = 0;
		var index = (selectedOptionIndex + delta + optionEntries.Count) % optionEntries.Count;
		SelectOption(index);
	}
	void ConfirmSelection()
	{
		Log.Print($"选择了选项{selectedOptionIndex} - {optionEntries[selectedOptionIndex].label}");
		CompleteActiveTask(selectedOptionIndex < 0 ? -1 : selectedOptionIndex);
	}
	void CompleteActiveTask(int result)
	{
		var task = this.task;
		this.task = null;
		ClearOptions();
		UpdateIconVisibility();
		task?.TrySetResult(result);
	}
	void UpdateIconVisibility()
	{
		var hasOptions = optionEntries.Count > 0 && optionsContainer.Visible;
		icon.Visible = !hasOptions;
		if (hasOptions) icon.SelfModulate = GameColors.transparent;
	}
}
