using Godot;
namespace RealismCombat.Nodes.Games;
[Tool]
public partial class PropertyNode : Node
{
	string title = null!;
	double current;
	double max;
	Label? label;
	ProgressBar? progressBar;
	public Label Label => label ??= GetNode<Label>("Label");
	public ProgressBar ProgressBar => progressBar ??= GetNode<ProgressBar>("ProgressBar");
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
	public (double current, double max) Value
	{
		get => (Current, Max);
		set
		{
			Max = value.max;
			Current = value.current;
		}
	}
	[Export]
	double Current
	{
		get => current;
		set
		{
			current = value;
			UpdateValue();
		}
	}
	[Export]
	double Max
	{
		get => max;
		set
		{
			max = value;
			UpdateValue();
		}
	}
	public override void _Ready()
	{
		base._Ready();
		UpdateTitle();
		UpdateValue();
	}
	void UpdateTitle() => Label?.Text = title;
	void UpdateValue()
	{
		ProgressBar?.MaxValue = Max;
		ProgressBar?.Value = Current;
	}
}
