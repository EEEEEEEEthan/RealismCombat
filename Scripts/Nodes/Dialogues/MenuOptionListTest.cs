using Godot;

public partial class MenuOptionListTest : Control
{
	MenuOptionList? optionList;
	MenuOptionList OptionList => optionList ??= GetNode<MenuOptionList>("%MenuOptionList");
	public override void _UnhandledInput(InputEvent @event)
	{
		var optionList = OptionList;
		var options = optionList.Options;
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

