using Godot;
[Tool, GlobalClass,]
public partial class BleedingNode : Control
{
	int index;
	double time;
	TextureRect? child;
	[Export]
	bool FlipH
	{
		get;
		set
		{
			field = value;
			child?.FlipH = value;
		}
	}
	public override void _Process(double delta)
	{
		base._Process(delta);
		time -= delta;
		if (time <= 0)
		{
			time = GD.Randf() * 0.3;
			index++;
			child!.Texture = SpriteTable.Bleeding[index % SpriteTable.Bleeding.Count];
		}
	}
	public override void _Ready()
	{
		base._Ready();
		AddChild(child = new()
		{
			StretchMode = TextureRect.StretchModeEnum.Scale,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
		});
		child.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		child.FlipH = FlipH;
		VisibilityChanged += () =>
		{
			index = 0;
			time = GD.Randf() * 0.3;
		};
	}
	public override Vector2 _GetMinimumSize() => new(16, 16);
}
