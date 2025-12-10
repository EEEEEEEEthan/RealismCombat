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
		Log.Print($"#{title},请选择(game_select_option)");
		foreach (var option in initialOptions) node.options.Add(option);
		node.BuildOptions();
		GameServer.McpCheckpoint();
		return node;
	}
	static string NormalizeDescription(MenuOption option)
	{
		var description = option.description;
		if (!option.disabled) return description;
		if (string.IsNullOrEmpty(description)) return "当前不可用";
		return description.StartsWith("当前不可用") ? description : $"当前不可用\n{description}";
	}
	readonly List<MenuOption> options = [];
	readonly TaskCompletionSource<int> taskCompletionSource = new();
	bool allowEscapeReturn;
	int returnOptionIndex = -1;
	int currentIndex = -1;
	[field: MaybeNull] MenuOptionList OptionContainer => field ??= GetNode<MenuOptionList>("%OptionContainer");
	[field: MaybeNull] Printer Printer => field ??= GetNode<Printer>("%Printer");
	[field: MaybeNull] Label TitleLabel => field ??= GetNode<Label>("%TitleLabel");
	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint()) return;
		if (options.Count > 0)
		{
			Select(0);
		}
	}
	public TaskAwaiter<int> GetAwaiter() => taskCompletionSource.Task.GetAwaiter();
	public void SelectAndConfirm(int index)
	{
		Select(index);
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
	protected override void HandleInput(InputEvent @event)
	{
		if (options.Count == 0) return;
		if (@event.IsActionPressed("ui_up"))
		{
			var index = currentIndex < 0 ? 0 : (currentIndex - 1 + options.Count) % options.Count;
			Select(index);
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_down"))
		{
			var index = currentIndex < 0 ? 0 : (currentIndex + 1) % options.Count;
			Select(index);
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
	void BuildOptions()
	{
		var optionResources = new List<MenuOptionResource>();
		for (var i = 0; i < options.Count; i++)
		{
			var option = options[i];
			option.description = NormalizeDescription(option);
			options[i] = option;
			optionResources.Add(new()
			{
				Text = option.title,
				Disabled = option.disabled,
			});
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
			optionResources.Add(new()
			{
				Text = returnOption.title,
				Disabled = returnOption.disabled,
			});
			Log.Print($"{returnOptionIndex} - {returnOption.title} {returnOption.description}");
		}
		OptionContainer.Options = optionResources.ToArray();
	}
	void Select(int index)
	{
		if (currentIndex == index) return;
		if (index < 0 || index >= options.Count) return;
		currentIndex = index;
		Printer.Text = options[currentIndex].description;
		Printer.VisibleCharacters = Printer.GetTotalCharacterCount();
		OptionContainer.Index = currentIndex;
	}
}
