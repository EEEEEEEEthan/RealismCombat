using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
namespace RealismCombat.Nodes.Dialogues;
public struct MenuOption
{
	public string title;
	public string description;
}
[Tool, GlobalClass,]
public partial class MenuDialogue : BaseDialogue
{
	readonly List<MenuOption> options = [];
	readonly List<Label> optionLabels = [];
	Container optionContainer;
	Control optionIndexer;
	PrinterNode printerNode;
	int currentIndex;
	TaskCompletionSource<int>? taskCompletionSource;
	public MenuDialogue()
	{
		var marginContainer = new MarginContainer();
		marginContainer.Name = "MarginContainer";
		AddChild(marginContainer);
		marginContainer.AddThemeConstantOverride("margin_left", 3);
		var hBoxContainer = new HBoxContainer();
		hBoxContainer.Name = "HBoxContainer";
		marginContainer.AddChild(hBoxContainer);
		hBoxContainer.AddThemeConstantOverride("separation", 4);
		{
			optionContainer = new VBoxContainer();
			optionContainer.Name = "VBoxContainer";
			hBoxContainer.AddChild(optionContainer);
		}
		{
			printerNode = new();
			hBoxContainer.AddChild(printerNode);
			printerNode.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		}
		var control = new Control();
		control.Name = "Control";
		AddChild(control);
		{
			optionIndexer = new();
			optionIndexer.Name = "Indexer";
			control.AddChild(optionIndexer);
			var textureRect = new TextureRect();
			textureRect.Name = "TextureRect";
			optionIndexer.AddChild(textureRect);
			textureRect.Position = new(-5, -4);
			textureRect.Size = new(8, 8);
			textureRect.Texture = SpriteTable.arrowRight;
			textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		}
	}
	public Task<int> StartTask()
	{
		taskCompletionSource = new();
		Log.Print("请选择(game_select_option)");
		GameServer.McpCheckpoint();
		return taskCompletionSource.Task;
	}
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
		Log.Print($"{options.Count - 1} - {option.title} {option.description}");
	}
	public override void HandleInput(InputEvent @event)
	{
		if (options.Count == 0) return;
		if (@event.IsActionPressed("ui_up"))
		{
			currentIndex--;
			if (currentIndex < 0) currentIndex = options.Count - 1;
			printerNode.VisibleCharacters = 0;
			UpdateUI();
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_down"))
		{
			currentIndex++;
			if (currentIndex >= options.Count) currentIndex = 0;
			printerNode.VisibleCharacters = 0;
			UpdateUI();
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_accept"))
		{
			taskCompletionSource?.TrySetResult(currentIndex);
			GetViewport().SetInputAsHandled();
			Close();
		}
	}
	public TaskAwaiter<int> GetAwaiter()
	{
		taskCompletionSource ??= new();
		return taskCompletionSource.Task.GetAwaiter();
	}
	public void SelectAndConfirm(int index)
	{
		Select(index);
		Confirm();
	}
	void Select(int index) => currentIndex = index;
	void Confirm()
	{
		taskCompletionSource?.TrySetResult(currentIndex);
		GetViewport().SetInputAsHandled();
		Close();
	}
	void UpdateUI()
	{
		if (options.Count == 0)
		{
			printerNode.Text = "";
			optionIndexer.Visible = false;
			return;
		}
		optionIndexer.Visible = true;
		printerNode.Text = options[currentIndex].description;
		printerNode.VisibleCharacters = 0;
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
