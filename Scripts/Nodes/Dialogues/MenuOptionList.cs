using System;
using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public partial class MenuOptionList : MarginContainer
{
	const int VisibleLines = 8;

	[Export] public string[]? Options
	{
		get => options;
		set
		{
			options = value ?? Array.Empty<string>();
			if (index >= options.Length) index = options.Length == 0 ? -1 : options.Length - 1;
			Rebuild();
		}
	}

	[Export] public int Index
	{
		get => index;
		set
		{
			var next = options.Length == 0 ? -1 : Mathf.Clamp(value, 0, options.Length - 1);
			if (index == next) return;
			index = next;
			KeepIndexVisible();
			Rebuild();
			CallDeferred(MethodName.UpdateIndicatorPosition);
		}
	}

	Control? indicatorHost;
	TextureRect? indicatorTexture;
	VBoxContainer? optionContainer;
	readonly List<Label> labelPool = new();
	readonly List<Label?> optionLabels = new();
	string[] options = Array.Empty<string>();
	int index = -1;
	int windowStart;
	bool building;

	public override void _EnterTree()
	{
		EnsureNodes();
	}

	public override void _Ready()
	{
		Rebuild();
		CallDeferred(MethodName.UpdateIndicatorPosition);
	}

	void EnsureNodes()
	{
		if (indicatorHost != null && optionContainer != null) return;

		if (GetNodeOrNull<Control>("IndicatorHost") is { } foundIndicator)
		{
			indicatorHost = foundIndicator;
		}
		else
		{
			indicatorHost = new Control { Name = "IndicatorHost" };
			AddChild(indicatorHost);
		}

		if (indicatorHost.GetNodeOrNull<TextureRect>("IndicatorTexture") is { } foundTexture)
		{
			indicatorTexture = foundTexture;
		}
		else
		{
			indicatorTexture = new TextureRect
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
		}

		if (GetNodeOrNull<VBoxContainer>("OptionContainer") is { } foundContainer)
		{
			optionContainer = foundContainer;
		}
		else
		{
			optionContainer = new VBoxContainer { Name = "OptionContainer" };
			AddChild(optionContainer);
		}

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

	void Rebuild()
	{
		if (building) return;
		building = true;
		EnsureNodes();

		if (indicatorHost == null || optionContainer == null || indicatorTexture == null)
		{
			building = false;
			return;
		}

		if (index < 0 && options.Length > 0) index = 0;
		KeepIndexVisible();

		var displayEntries = BuildDisplayEntries();
		PrepareLabelPool(displayEntries.Count);
		ResetOptionLabels();
		HideAllLabels();
		FillLabels(displayEntries);
		UpdateIndicatorPosition();
		CallDeferred(MethodName.UpdateIndicatorPosition);
		building = false;
	}

	List<(string text, int optionIndex)> BuildDisplayEntries()
	{
		if (options.Length == 0)
		{
			index = -1;
			windowStart = 0;
			indicatorHost!.Visible = false;
			building = false;
			return new();
		}

		return BuildEntriesForStart(windowStart);
	}

	List<(string text, int optionIndex)> BuildEntriesForStart(int start)
	{
		if (index < 0) index = 0;

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

		var entries = new List<(string text, int optionIndex)>();
		if (showTopEllipsis) entries.Add(($"...+{hiddenAbove}", -1));
		for (var i = 0; i < displayCount; i++)
		{
			var optionIndex = start + i;
			entries.Add((options[optionIndex], optionIndex));
		}
		if (showBottomEllipsis) entries.Add(($"...+{hiddenAfter}", -1));
		return entries;
	}

	void PrepareLabelPool(int needed)
	{
		while (labelPool.Count < needed)
		{
			var label = new Label();
			labelPool.Add(label);
			optionContainer!.AddChild(label);
		}
	}

	void ResetOptionLabels()
	{
		optionLabels.Clear();
		for (var i = 0; i < options.Length; i++) optionLabels.Add(null);
	}

	void HideAllLabels()
	{
		foreach (var label in labelPool)
			label.Visible = false;
	}

	void FillLabels(List<(string text, int optionIndex)> entries)
	{
		for (var i = 0; i < entries.Count; i++)
		{
			var (text, optionIndex) = entries[i];
			var label = labelPool[i];
			label.Text = text;
			label.Visible = true;
			if (optionIndex >= 0 && optionIndex < optionLabels.Count)
			{
				optionLabels[optionIndex] = label;
			}
		}

	}

	void KeepIndexVisible()
	{
		if (options.Length == 0)
		{
			windowStart = 0;
			return;
		}

		windowStart = FindWindowStart(index);
	}

	int FindWindowStart(int targetIndex)
	{
		var maxStart = Math.Max(0, options.Length - VisibleLines);
		var bestStart = 0;
		var bestScore = -1;
		for (var candidate = 0; candidate <= maxStart; candidate++)
		{
			if (!ContainsIndex(candidate, targetIndex)) continue;
			var (showTop, showBottom) = EvaluateEllipsis(candidate);
			var score = (showTop ? 1 : 0) + (showBottom ? 1 : 0);
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
		{
			if (entry.optionIndex == targetIndex) return true;
		}
		return false;
	}

	(bool showTop, bool showBottom) EvaluateEllipsis(int start)
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
		return (showTopEllipsis, showBottomEllipsis);
	}

	void UpdateIndicatorPosition()
	{
		EnsureNodes();
		var host = indicatorHost!;
		var texture = indicatorTexture!;
		if (index < 0 || index >= optionLabels.Count) { host.Visible = false; return; }
		var label = optionLabels[index];
		if (label == null || !IsInstanceValid(label)) { host.Visible = false; return; }

		texture.Texture = SpriteTable.arrowRight;
		host.Visible = true;
		var local = optionContainer!.GetGlobalTransformWithCanvas().AffineInverse() * label.GetGlobalTransformWithCanvas().Origin;
		var texSize = indicatorTexture!.Texture?.GetSize() ?? new Vector2(8f, 8f);
		if (indicatorTexture.Size == Vector2.Zero) indicatorTexture.Size = texSize;
		var indicatorWidth = indicatorTexture.Size.X > 0 ? indicatorTexture.Size.X : texSize.X;
		var targetX = -indicatorWidth - 2f;
		var localY = local.Y + label.Size.Y / 2f - indicatorTexture.Size.Y / 2f;
		indicatorTexture.Position = new Vector2(targetX, localY);
	}
}

