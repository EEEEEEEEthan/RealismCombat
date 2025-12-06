using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
public struct MenuOption
{
	public string title;
	public string description;
	public bool disabled;
}
[Tool,]
public partial class MenuDialogue : BaseDialogue
{
	public static MenuDialogue Create(string title, IEnumerable<MenuOption> initialOptions, bool allowEscapeReturn = false)
	{
		PackedScene scene = ResourceTable.menuDialogueScene;
		var node = scene.Instantiate<MenuDialogue>();
		node.allowEscapeReturn = allowEscapeReturn;
		node.TitleLabel.Text = title;
		node.options.Clear();
		foreach (var option in initialOptions) node.options.Add(option);
		node.BuildOptions();
		return node;
	}
	readonly List<MenuOption> options = [];
	readonly List<Label> optionLabels = [];
	readonly TaskCompletionSource<int> taskCompletionSource = new();
	bool allowEscapeReturn;
	int returnOptionIndex = -1;
	int currentIndex = -1;
	[field: MaybeNull] Container OptionContainer => field ??= GetNode<Container>("MarginContainer/HBoxContainer/VBoxContainer");
	[field: MaybeNull] Printer Printer => field ??= GetNode<Printer>("MarginContainer/HBoxContainer/Printer");
	[field: MaybeNull] Control OptionIndexer => field ??= GetNode<Control>("Control/Indexer");
	[field: MaybeNull] TextureRect IndexerTextureRect => field ??= GetNode<TextureRect>("Control/Indexer/TextureRect");
	[field: MaybeNull] Label TitleLabel => field ??= GetNode<Label>("Control/TitleContainer/Title");
	public override void _Ready()
	{
		base._Ready();
		IndexerTextureRect.Texture = SpriteTable.arrowRight;
		if (options.Count > 0)
		{
			var firstEnabledIndex = -1;
			for (var i = 0; i < options.Count; i++)
				if (!options[i].disabled)
				{
					firstEnabledIndex = i;
					break;
				}
			if (firstEnabledIndex >= 0) Select(firstEnabledIndex);
		}
		Log.Print("请选择(game_select_option)");
		GameServer.McpCheckpoint();
		ItemRectChanged += UpdateIndexer;
		UpdateIndexer();
	}
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
			var attempts = 0;
			do
			{
				if (--index < 0) index = options.Count - 1;
				attempts++;
			} while (options[index].disabled && attempts < options.Count);
			if (!options[index].disabled) Select(index);
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_down"))
		{
			var index = currentIndex;
			var attempts = 0;
			do
			{
				if (++index >= options.Count) index = 0;
				attempts++;
			} while (options[index].disabled && attempts < options.Count);
			if (!options[index].disabled) Select(index);
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_accept"))
		{
			if (currentIndex >= 0 && currentIndex < options.Count && !options[currentIndex].disabled)
			{
				GetViewport().SetInputAsHandled();
				var index = currentIndex;
				Close();
				taskCompletionSource.TrySetResult(index);
			}
		}
		else if (allowEscapeReturn && @event.IsActionPressed("ui_cancel"))
		{
			GetViewport().SetInputAsHandled();
			var index = returnOptionIndex;
			Close();
			taskCompletionSource.TrySetResult(index);
		}
	}
	void UpdateTitle() { }
	void BuildOptions()
	{
		for (var i = 0; i < options.Count; i++)
		{
			var option = options[i];
			var label = new Label
			{
				Text = option.title,
			};
			if (option.disabled) label.Modulate = new(178f / 255f, 178f / 255f, 178f / 255f);
			OptionContainer.AddChild(label);
			optionLabels.Add(label);
			Log.Print($"{i} - {option.title} {option.description}");
		}
		if (allowEscapeReturn)
		{
			returnOptionIndex = options.Count;
			var returnOption = new MenuOption
			{
				title = "返回",
				description = string.Empty,
				disabled = false,
			};
			options.Add(returnOption);
			var returnLabel = new Label
			{
				Text = returnOption.title,
			};
			OptionContainer.AddChild(returnLabel);
			optionLabels.Add(returnLabel);
			Log.Print($"{returnOptionIndex} - {returnOption.title} {returnOption.description}");
		}
	}
	void Select(int index)
	{
		if (currentIndex == index) return;
		if (index < 0 || index >= options.Count) return;
		currentIndex = index;
		Printer.Text = options[currentIndex].description;
		Printer.VisibleCharacters = 0;
		UpdateIndexer();
	}
	void UpdateIndexer()
	{
		if (currentIndex < 0 || currentIndex >= optionLabels.Count) return;
		var selectedLabel = optionLabels[currentIndex];
		OptionIndexer.GlobalPosition = new(
			OptionIndexer.GlobalPosition.X,
			selectedLabel.GlobalPosition.Y + selectedLabel.Size.Y / 2 - OptionIndexer.Size.Y / 2
		);
	}
	void Confirm()
	{
		if (currentIndex < 0 || currentIndex >= options.Count) return;
		Log.Print($"选择了选项{currentIndex} - {options[currentIndex].title}");
		if (options[currentIndex].disabled)
		{
			Log.Print("选项不可用。");
			GameServer.McpCheckpoint();
			return;
		}
		GetViewport().SetInputAsHandled();
		Close();
		taskCompletionSource.TrySetResult(currentIndex);
	}
}
