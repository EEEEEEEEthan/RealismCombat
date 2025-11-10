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
	readonly TaskCompletionSource<int> taskCompletionSource = new();
	readonly bool allowEscapeReturn;
	readonly int returnOptionIndex = -1;
	Container optionContainer;
	Control optionIndexer;
	Printer printer;
	int currentIndex = -1;
	public MenuDialogue(IEnumerable<MenuOption> initialOptions, bool allowEscapeReturn)
	{
		this.allowEscapeReturn = allowEscapeReturn;
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
			printer = new();
			hBoxContainer.AddChild(printer);
			printer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
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
		foreach (var option in initialOptions)
		{
			options.Add(option);
			var label = new Label
			{
				Text = option.title,
			};
			optionContainer.AddChild(label);
			optionLabels.Add(label);
			Log.Print($"{options.Count - 1} - {option.title} {option.description}");
		}
		if (allowEscapeReturn)
		{
			returnOptionIndex = options.Count;
			var returnOption = new MenuOption
			{
				title = "返回",
				description = string.Empty,
			};
			options.Add(returnOption);
			var returnLabel = new Label
			{
				Text = returnOption.title,
			};
			optionContainer.AddChild(returnLabel);
			optionLabels.Add(returnLabel);
			Log.Print($"{returnOptionIndex} - {returnOption.title} {returnOption.description}");
		}
		if (options.Count > 0) Select(0);
		Log.Print("请选择(game_select_option)");
		GameServer.McpCheckpoint();
		Ready += UpdateIndexer;
		ItemRectChanged += UpdateIndexer;
	}
	public MenuDialogue(IEnumerable<MenuOption> initialOptions) : this(initialOptions, false) { }
	MenuDialogue() : this([], false) { }
	public TaskAwaiter<int> GetAwaiter() => taskCompletionSource.Task.GetAwaiter();
	public void SelectAndConfirm(int index)
	{
		Select(index);
		Confirm();
	}
	protected override void HandleInput(InputEvent @event)
	{
		if (options.Count == 0) return;
		if (@event.IsActionPressed("ui_up"))
		{
			var index = currentIndex;
			if (--index < 0) index = options.Count - 1;
			Select(index);
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_down"))
		{
			var index = currentIndex;
			if (++index >= options.Count) index = 0;
			Select(index);
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_accept"))
		{
			GetViewport().SetInputAsHandled();
			var index = currentIndex;
			Close();
			taskCompletionSource.TrySetResult(index);
		}
		else if (allowEscapeReturn && @event.IsActionPressed("ui_cancel"))
		{
			GetViewport().SetInputAsHandled();
			var index = returnOptionIndex;
			Close();
			taskCompletionSource.TrySetResult(index);
		}
	}
	void Select(int index)
	{
		if (currentIndex == index) return;
		if (index < 0 || index >= options.Count) return;
		currentIndex = index;
		printer.Text = options[currentIndex].description;
		printer.VisibleCharacters = 0;
		UpdateIndexer();
	}
	void UpdateIndexer()
	{
		if (currentIndex < 0 || currentIndex >= optionLabels.Count) return;
		var selectedLabel = optionLabels[currentIndex];
		optionIndexer.GlobalPosition = new(
			optionIndexer.GlobalPosition.X,
			selectedLabel.GlobalPosition.Y + selectedLabel.Size.Y / 2 - optionIndexer.Size.Y / 2
		);
	}
	void Confirm()
	{
		GetViewport().SetInputAsHandled();
		Close();
		taskCompletionSource.TrySetResult(currentIndex);
	}
}
