using System;
using Godot;

public partial class MenuOptionListTest : Control
{
	MenuOptionList? optionList;
	MenuOptionList? OptionList => optionList ??= GetNodeOrNull<MenuOptionList>("%MenuOptionList");
	public override void _Ready()
	{
		if (OptionList is not { } optionList) return;
		optionList.Options =
		[
			new MenuOptionResource { text = "aaaa", disabled = false },
			new MenuOptionResource { text = "bbbb", disabled = true },
			new MenuOptionResource { text = "cccc", disabled = false },
			new MenuOptionResource { text = "dddd", disabled = false },
			new MenuOptionResource { text = "eeee", disabled = true },
			new MenuOptionResource { text = "ffff", disabled = false },
			new MenuOptionResource { text = "gggg", disabled = false },
			new MenuOptionResource { text = "hhhh", disabled = false },
			new MenuOptionResource { text = "iiii", disabled = false },
			new MenuOptionResource { text = "jjjj", disabled = false },
			new MenuOptionResource { text = "kkkk", disabled = false },
		];
		optionList.Index = 0;
	}
	public override void _UnhandledInput(InputEvent @event)
	{
		if (OptionList is not { } optionList) return;
		var options = optionList.Options ?? [];
		if (options.Length == 0) return;
		if (@event.IsActionPressed("ui_up"))
		{
			var targetIndex = optionList.Index < 0 ? 0 : optionList.Index - 1;
			optionList.Index = targetIndex;
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_down"))
		{
			var maxIndex = options.Length - 1;
			var targetIndex = optionList.Index < 0 ? 0 : optionList.Index + 1;
			if (targetIndex > maxIndex) targetIndex = maxIndex;
			optionList.Index = targetIndex;
			GetViewport().SetInputAsHandled();
		}
	}
}

