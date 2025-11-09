using Godot;
namespace RealismCombat.Nodes.Games;
[Tool]
public partial class PropertyNode : Node
{
	string title = null!;
	(double current, double max) value;
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
		get => value;
		set
		{
			this.value = value;
			UpdateValue();
		}
	}
	public override void _Ready()
	{
		base._Ready();
		UpdateTitle();
		UpdateValue();
	}
	void UpdateTitle() => Label.Text = title;
	void UpdateValue()
	{
		ProgressBar.MaxValue = value.max;
		ProgressBar.Value = value.current;
	}
}
