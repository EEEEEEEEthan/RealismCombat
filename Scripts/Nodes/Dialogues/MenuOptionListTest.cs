using System;
using Godot;

public partial class MenuOptionListTest : Control
{
	MenuOptionList? optionList;
	MenuOptionList? OptionList => optionList ??= GetNodeOrNull<MenuOptionList>("%MenuOptionList");
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

