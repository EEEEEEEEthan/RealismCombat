using System;
using System.Collections.Generic;
using Godot;
namespace RealismCombat.Nodes;
public struct MenuOption
{
	public string title;
	public string description;
	public Action onClick;
}
public partial class MenuDialogue : PanelContainer
{
	readonly List<MenuOption> options = [];
	readonly List<Label> optionLabels = [];
	/// <summary>
	///     option容器, 放labels
	/// </summary>
	[Export] Container optionContainer = null!;
	/// <summary>
	///     一个三角箭头,坐标与当前选择的option对齐
	/// </summary>
	[Export] Control optionIndexer = null!;
	/// <summary>
	///     显示description
	/// </summary>
	[Export] Printer printer = null!;
	int currentIndex;
	public override void _Ready()
	{
		base._Ready();
		UpdateUI();
	}
	public void ClearOptions()
	{
		options.Clear();
		foreach (var label in optionLabels) label.QueueFree();
		optionLabels.Clear();
		currentIndex = 0;
		UpdateUI();
	}
	public void AddOption(MenuOption option)
	{
		options.Add(option);
		var label = new Label
		{
			Text = option.title,
		};
		optionContainer.AddChild(label);
		optionLabels.Add(label);
		if (options.Count == 1)
		{
			currentIndex = 0;
			UpdateUI();
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (options.Count == 0) return;
		if (@event.IsActionPressed("ui_up"))
		{
			currentIndex--;
			if (currentIndex < 0) currentIndex = options.Count - 1;
			printer.VisibleCharacters = 0;
			UpdateUI();
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_down"))
		{
			currentIndex++;
			if (currentIndex >= options.Count) currentIndex = 0;
			printer.VisibleCharacters = 0;
			UpdateUI();
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_accept"))
		{
			options[currentIndex].onClick?.Invoke();
			GetViewport().SetInputAsHandled();
		}
	}
	void UpdateUI()
	{
		if (options.Count == 0)
		{
			printer.Text = "";
			optionIndexer.Visible = false;
			return;
		}
		optionIndexer.Visible = true;
		printer.Text = options[currentIndex].description;
		printer.VisibleCharacters = 0;
		// 更新箭头位置，对齐到当前选中的选项
		if (currentIndex < optionLabels.Count)
		{
			var selectedLabel = optionLabels[currentIndex];
			optionIndexer.GlobalPosition = new(
				optionIndexer.GlobalPosition.X,
				selectedLabel.GlobalPosition.Y + selectedLabel.Size.Y / 2 - optionIndexer.Size.Y / 2
			);
		}
	}
}
