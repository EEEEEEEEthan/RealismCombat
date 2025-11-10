using Godot;
namespace RealismCombat.Nodes.Games;
[Tool, GlobalClass,]
public partial class BleedingNode : TextureRect
{
	int index;
	double time;
	public override void _Process(double delta)
	{
		base._Process(delta);
		time -= delta;
		if (time <=0)
		{
			time = GD.Randf() * 0.3;
			index++;
			Texture = SpriteTable.bleeding[index % SpriteTable.bleeding.Count];
		}
	}
	public override void _Ready()
	{
		base._Ready();
		VisibilityChanged += () =>
		{
			index = 0;
			time = GD.Randf() * 0.3;
		};
	}
}
