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
	const int columnCapacity = 6;
	readonly List<List<MenuOption>> options = [];
	readonly List<List<Label>> optionLabels = [];
	readonly List<VBoxContainer> optionColumns = [];
	readonly TaskCompletionSource<int> taskCompletionSource = new();
	Container optionContainer;
	Control optionIndexer;
	PrinterNode printerNode;
	Vector2I currentIndex = new(-1, -1);
	public MenuDialogue(IEnumerable<MenuOption> initialOptions)
	{
		var marginContainer = new MarginContainer();
		marginContainer.Name = "MarginContainer";
		AddChild(marginContainer);
		marginContainer.AddThemeConstantOverride("margin_left", 3);
		var hBoxContainer = new HBoxContainer();
		hBoxContainer.Name = "HBoxContainer";
		marginContainer.AddChild(hBoxContainer);
		hBoxContainer.AddThemeConstantOverride("separation", 4);
		optionContainer = new HBoxContainer();
		optionContainer.Name = "HBoxContainer";
		hBoxContainer.AddChild(optionContainer);
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
		foreach (var option in initialOptions) AddOption(option);
		if (TotalOptionCount() > 0)
		{
			Select(0);
			Log.Print("请选择(game_select_option)");
			GameServer.McpCheckpoint();
		}
		Ready += UpdateIndexer;
		ItemRectChanged += UpdateIndexer;
	}
	MenuDialogue() : this([]) { }
	public TaskAwaiter<int> GetAwaiter() => taskCompletionSource.Task.GetAwaiter();
	public void SelectAndConfirm(int index)
	{
		var coordinate = GetCoordinateFromLinear(index);
		if (!IsValidIndex(coordinate)) return;
		Select(coordinate);
		Confirm();
	}
	protected override void HandleInput(InputEvent @event)
	{
		if (TotalOptionCount() == 0) return;
		if (@event.IsActionPressed("ui_up"))
		{
			TryMoveVertical(-1);
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_down"))
		{
			TryMoveVertical(1);
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_left"))
		{
			TryMoveHorizontal(-1);
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_right"))
		{
			TryMoveHorizontal(1);
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_accept"))
		{
			GetViewport().SetInputAsHandled();
			var index = GetLinearIndex(currentIndex);
			Close();
			taskCompletionSource.TrySetResult(index);
		}
	}
	void Select(int index)
	{
		var coordinate = GetCoordinateFromLinear(index);
		Select(coordinate);
	}
	void Select(Vector2I index)
	{
		if (!IsValidIndex(index)) return;
		if (currentIndex == index) return;
		currentIndex = index;
		printerNode.Text = options[currentIndex.X][currentIndex.Y].description;
		printerNode.VisibleCharacters = 0;
		UpdateIndexer();
	}
	void UpdateIndexer()
	{
		if (!IsValidIndex(currentIndex)) return;
		var selectedLabel = optionLabels[currentIndex.X][currentIndex.Y];
		optionIndexer.GlobalPosition = new(
			optionIndexer.GlobalPosition.X,
			selectedLabel.GlobalPosition.Y + selectedLabel.Size.Y / 2 - optionIndexer.Size.Y / 2
		);
	}
	void Confirm()
	{
		if (!IsValidIndex(currentIndex)) return;
		GetViewport().SetInputAsHandled();
		Close();
		taskCompletionSource.TrySetResult(GetLinearIndex(currentIndex));
	}
	void AddOption(MenuOption option)
	{
		if (options.Count == 0 || options[^1].Count >= columnCapacity)
		{
			options.Add(new());
			optionLabels.Add(new());
			var column = new VBoxContainer();
			column.Name = $"VBoxContainer{options.Count - 1}";
			optionColumns.Add(column);
			optionContainer.AddChild(column);
		}
		var columnIndex = options.Count - 1;
		options[columnIndex].Add(option);
		var label = new Label
		{
			Text = option.title,
		};
		optionColumns[columnIndex].AddChild(label);
		optionLabels[columnIndex].Add(label);
		Log.Print($"{TotalOptionCount() - 1} - {option.title} {option.description}");
	}
	int TotalOptionCount()
	{
		var total = 0;
		foreach (var column in options) total += column.Count;
		return total;
	}
	int GetLinearIndex(Vector2I index)
	{
		if (!IsValidIndex(index)) return -1;
		var total = 0;
		for (var x = 0; x < index.X; x++) total += options[x].Count;
		return total + index.Y;
	}
	Vector2I GetCoordinateFromLinear(int value)
	{
		if (value < 0 || value >= TotalOptionCount()) return new(-1, -1);
		var remaining = value;
		for (var x = 0; x < options.Count; x++)
		{
			if (remaining < options[x].Count) return new(x, remaining);
			remaining -= options[x].Count;
		}
		return new(-1, -1);
	}
	bool IsValidIndex(Vector2I index)
	{
		if (index.X < 0 || index.Y < 0) return false;
		if (index.X >= options.Count) return false;
		if (index.Y >= options[index.X].Count) return false;
		return true;
	}
	void TryMoveVertical(int delta)
	{
		if (!IsValidIndex(currentIndex)) return;
		var columnIndex = currentIndex.X;
		var rowCount = options[columnIndex].Count;
		if (rowCount == 0) return;
		var rowIndex = (currentIndex.Y + delta + rowCount) % rowCount;
		Select(new Vector2I(columnIndex, rowIndex));
	}
	void TryMoveHorizontal(int delta)
	{
		if (!IsValidIndex(currentIndex)) return;
		var columnCount = options.Count;
		if (columnCount == 0) return;
		var columnIndex = currentIndex.X;
		var rowIndex = currentIndex.Y;
		var steps = 0;
		var nextColumn = columnIndex;
		while (steps < columnCount)
		{
			nextColumn = (nextColumn + delta + columnCount) % columnCount;
			steps++;
			if (nextColumn == columnIndex) continue;
			if (rowIndex < options[nextColumn].Count)
			{
				Select(new Vector2I(nextColumn, rowIndex));
				return;
			}
		}
	}
}
