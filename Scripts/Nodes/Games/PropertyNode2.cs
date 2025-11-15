using System.Diagnostics.CodeAnalysis;
using Godot;
namespace RealismCombat.Nodes.Games;
[Tool]
public partial class PropertyNode2 : Control
{
	const float FlashDuration = 0.2f;
	string title = null!;
	SceneTreeTimer? flashTimer;
	[field: AllowNull, MaybeNull,] public Label Label => field ??= GetNodeOrNull<Label>("Label");
	[field: AllowNull, MaybeNull,] public ProgressBar ProgressBar => field ??= GetNodeOrNull<ProgressBar>("ProgressBar");
	[field: AllowNull, MaybeNull,] public Label ValueLabel => field ??= GetNodeOrNull<Label>("ProgressBar/Label");
	public double Progress => Max == 0 ? 0 : Current / Max;
	[Export]
	public string Title
	{
		get => title;
		set
		{
			title = value;
			UpdateTitle();
		}
	}
	public (int current, int max) Value
	{
		get => ((int)Current, (int)Max);
		set
		{
			Max = value.max;
			Current = value.current;
		}
	}
	[Export]
	double Current
	{
		get;
		set
		{
			field = value;
			UpdateValue();
		}
	}
	[Export]
	double Max
	{
		get;
		set
		{
			field = value;
			UpdateValue();
		}
	}
	public override void _Ready()
	{
		base._Ready();
		UpdateTitle();
		UpdateValue();
	}
	/// <summary>
	///     闪烁红色，持续0.2秒
	/// </summary>
	public void FlashRed()
	{
		var originalModulate = Modulate;
		var flashColor = GameColors.pinkGradient[^1];
		Modulate = flashColor;
		flashTimer = GetTree().CreateTimer(FlashDuration);
		flashTimer.Timeout += () =>
		{
			Modulate = originalModulate;
			flashTimer = null;
		};
	}
	void UpdateTitle()
	{
		if (!IsNodeReady()) return;
		Label.Text = title;
	}
	void UpdateValue()
	{
		if (!IsNodeReady()) return;
		ProgressBar.MaxValue = Max;
		ProgressBar.Value = Current;
		var currentInt = (int)Current;
		var maxInt = (int)Max;
		ValueLabel.Text = $"{currentInt}/{maxInt}";
	}
}
