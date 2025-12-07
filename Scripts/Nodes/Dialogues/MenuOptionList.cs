using System.Collections.Generic;
using Godot;
[Tool]
[GlobalClass]
public partial class MenuOptionList : MarginContainer
{
	const int VisibleLines = 8;
	readonly List<Label> optionLabels = [];
	Control indicatorHost;
	TextureRect indicatorTexture;
	VBoxContainer optionContainer;
	string[] options = [];
	[Export]
	public string[]? Options
	{
		get => options;
		set
		{
			options = value ?? [];
			if (Index >= options.Length) Index = options.Length == 0 ? -1 : options.Length - 1;
			Rebuild();
		}
	}
	[Export]
	int Index
	{
		get;
		set
		{
			var next = options.Length == 0 ? -1 : Mathf.Clamp(value, 0, options.Length - 1);
			field = next;
			if (TopVisibleIndex >= next) TopVisibleIndex = Mathf.Max(next - 1, 0);
			if (TopVisibleIndex + VisibleLines - 1 <= next) TopVisibleIndex = Mathf.Min(next - VisibleLines + 2, Mathf.Max(options.Length - VisibleLines, 0));
			Rebuild();
		}
	} = -1;
	int TopVisibleIndex
	{
		get;
		set
		{
			System.ArgumentOutOfRangeException.ThrowIfNegative(value);
			field = value;
		}
	}
	public MenuOptionList()
	{
		indicatorHost = new() { Name = "IndicatorHost", };
		AddChild(indicatorHost);
		indicatorTexture = new()
		{
			Name = "IndicatorTexture",
			StretchMode = TextureRect.StretchModeEnum.KeepCentered,
			OffsetLeft = 0,
			OffsetTop = 0,
			OffsetRight = 0,
			OffsetBottom = 0,
			Position = Vector2.Zero,
			Texture = SpriteTable.arrowRight,
		};
		indicatorHost.AddChild(indicatorTexture);
		optionContainer = new() { Name = "OptionContainer", };
		AddChild(optionContainer);
		indicatorHost.AnchorLeft = 0;
		indicatorHost.AnchorRight = 1;
		indicatorHost.AnchorTop = 0;
		indicatorHost.AnchorBottom = 1;
		indicatorHost.SetAnchorsPreset(LayoutPreset.FullRect);
		indicatorHost.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		indicatorHost.SizeFlagsVertical = SizeFlags.ExpandFill;
		indicatorHost.OffsetLeft = 0;
		indicatorHost.OffsetTop = 0;
		indicatorHost.OffsetRight = 0;
		indicatorHost.OffsetBottom = 0;
		optionContainer.AnchorLeft = 0;
		optionContainer.AnchorTop = 0;
		optionContainer.AnchorRight = 1;
		optionContainer.AnchorBottom = 1;
		optionContainer.OffsetLeft = 10;
		optionContainer.OffsetTop = 0;
		optionContainer.OffsetRight = 0;
		optionContainer.OffsetBottom = 0;
		optionContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		optionContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
		for (var i = 0; i < VisibleLines; i++)
		{
			var label = new Label
			{
				Name = $"OptionLabel{i}",
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ShrinkBegin,
			};
			optionContainer.AddChild(label);
			optionLabels.Add(label);
		}
	}
	public override void _Ready() => CallDeferred(nameof(Rebuild));
	void Rebuild()
	{
		for (var i = 0; i < VisibleLines; i++)
			switch (i)
			{
				case 0 when TopVisibleIndex > 0:
					optionLabels[i].Text = $"...(+{TopVisibleIndex + 1})";
					break;
				case VisibleLines - 1 when TopVisibleIndex + VisibleLines < options.Length:
					optionLabels[i].Text = $"...(+{options.Length - (TopVisibleIndex + VisibleLines - 1)})";
					break;
				default:
				{
					var optionIndex = TopVisibleIndex + i;
					if (optionIndex >= options.Length)
					{
						optionLabels[i].Text = "";
						continue;
					}
					optionLabels[i].Text = options[optionIndex];
					break;
				}
			}
		if (Index >= 0) indicatorTexture.GlobalPosition = optionLabels[Index - TopVisibleIndex].GlobalPosition + new Vector2(-indicatorTexture.Size.X, 2);
	}
}
