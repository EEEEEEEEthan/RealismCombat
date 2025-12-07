using System;
using System.Collections.Generic;
using Godot;
[Tool]
[GlobalClass]
public partial class MenuOptionList : MarginContainer
{
	const int VisibleLines = 8;
	readonly List<Label?> optionLabels = [];
	Control indicatorHost;
	TextureRect indicatorTexture;
	VBoxContainer optionContainer;
	string[] options = [];
	int index = -1;
	int windowStart;
	[Export]
	public string[]? Options
	{
		get => options;
		set
		{
			options = value ?? [];
			if (index >= options.Length) index = options.Length == 0 ? -1 : options.Length - 1;
			Rebuild();
		}
	}
	[Export]
	public int Index
	{
		get => index;
		set
		{
			var next = options.Length == 0 ? -1 : Mathf.Clamp(value, 0, options.Length - 1);
			if (index == next) return;
			index = next;
			Rebuild();
			CallDeferred(MethodName.UpdateIndicatorPosition);
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
	}
	public override void _Ready()
	{
		Rebuild();
		CallDeferred(MethodName.UpdateIndicatorPosition);
	}
	void Rebuild()
	{
		if (index < 0 && options.Length > 0) index = 0;
		windowStart = options.Length == 0 ? 0 : FindWindowStart(index);
		var displayEntries = BuildDisplayEntries();
		ResetOptionLabels();
		FillLabels(displayEntries);
		UpdateIndicatorPosition();
		CallDeferred(MethodName.UpdateIndicatorPosition);
	}
	List<(string text, int optionIndex)> BuildDisplayEntries()
	{
		if (options.Length == 0)
		{
			index = -1;
			windowStart = 0;
			indicatorHost!.Visible = false;
			return [];
		}
		return BuildEntriesForStart(windowStart);
	}
	List<(string text, int optionIndex)> BuildEntriesForStart(int start)
	{
		if (index < 0) index = 0;
		var layout = EvaluateWindow(start);
		var entries = new List<(string text, int optionIndex)>();
		if (layout.showTop) entries.Add(($"...+{layout.hiddenAbove}", -1));
		for (var i = 0; i < layout.displayCount; i++)
		{
			var optionIndex = start + i;
			entries.Add((options[optionIndex], optionIndex));
		}
		if (layout.showBottom) entries.Add(($"...+{layout.hiddenAfter}", -1));
		return entries;
	}
	void ResetOptionLabels()
	{
		optionLabels.Clear();
		for (var i = 0; i < options.Length; i++) optionLabels.Add(null);
	}
	void FillLabels(List<(string text, int optionIndex)> entries)
	{
		RemoveExistingLabels();
		for (var i = 0; i < entries.Count; i++)
		{
			(var text, var optionIndex) = entries[i];
			var label = new Label { Text = text, };
			optionContainer!.AddChild(label);
			if (optionIndex >= 0 && optionIndex < optionLabels.Count) optionLabels[optionIndex] = label;
		}
	}
	void RemoveExistingLabels()
	{
		var children = optionContainer!.GetChildren();
		foreach (var child in children)
		{
			optionContainer.RemoveChild(child);
			child.QueueFree();
		}
	}
	int FindWindowStart(int targetIndex)
	{
		var maxStart = Math.Max(0, options.Length - VisibleLines);
		var bestStart = 0;
		var bestScore = int.MinValue;
		for (var candidate = 0; candidate <= maxStart; candidate++)
		{
			if (!ContainsIndex(candidate, targetIndex)) continue;
			var layout = EvaluateWindow(candidate);
			var score = (layout.showTop ? layout.hiddenAbove : 0) + (layout.showBottom ? layout.hiddenAfter : 0);
			if (score > bestScore)
			{
				bestScore = score;
				bestStart = candidate;
			}
			else if (score == bestScore && candidate < bestStart)
			{
				bestStart = candidate;
			}
		}
		return bestStart;
	}
	bool ContainsIndex(int candidateStart, int targetIndex)
	{
		var entries = BuildEntriesForStart(candidateStart);
		foreach (var entry in entries)
			if (entry.optionIndex == targetIndex)
				return true;
		return false;
	}
	(bool showTop, bool showBottom, int hiddenAbove, int hiddenAfter, int displayCount) EvaluateWindow(int start)
	{
		var hiddenAbove = start;
		var showTopEllipsis = hiddenAbove > 1;
		var remainingSlots = VisibleLines - (showTopEllipsis ? 1 : 0);
		var displayCount = Math.Min(remainingSlots, options.Length - start);
		var hiddenAfter = options.Length - (start + displayCount);
		var showBottomEllipsis = hiddenAfter > 1;
		if (showBottomEllipsis && displayCount == remainingSlots)
		{
			displayCount = Math.Max(0, displayCount - 1);
			hiddenAfter = options.Length - (start + displayCount);
			showBottomEllipsis = hiddenAfter > 1;
		}
		return (showTopEllipsis, showBottomEllipsis, hiddenAbove, hiddenAfter, displayCount);
	}
	void UpdateIndicatorPosition()
	{
		var host = indicatorHost!;
		var texture = indicatorTexture!;
		if (index < 0 || index >= optionLabels.Count)
		{
			host.Visible = false;
			return;
		}
		var label = optionLabels[index];
		if (label == null || !IsInstanceValid(label))
		{
			host.Visible = false;
			return;
		}
		texture.Texture = SpriteTable.arrowRight;
		host.Visible = true;
		var local = optionContainer!.GetGlobalTransformWithCanvas().AffineInverse() * label.GetGlobalTransformWithCanvas().Origin;
		var texSize = indicatorTexture!.Texture?.GetSize() ?? new Vector2(8f, 8f);
		if (indicatorTexture.Size == Vector2.Zero) indicatorTexture.Size = texSize;
		var indicatorWidth = indicatorTexture.Size.X > 0 ? indicatorTexture.Size.X : texSize.X;
		var targetX = -indicatorWidth - 2f;
		var localY = local.Y + label.Size.Y / 2f - indicatorTexture.Size.Y / 2f;
		indicatorTexture.Position = new(targetX, localY);
	}
}
