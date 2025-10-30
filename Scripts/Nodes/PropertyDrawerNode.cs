using Godot;
namespace RealismCombat.Nodes;
partial class PropertyDrawerNode : Node
{
	public (int value, int max) property = (10, 10);
	public string title;
	[Export] Label name = null!;
	[Export] Control value = null!;
	public override void _Process(double delta)
	{
		name.Text = title;
		value.CustomMinimumSize = value.CustomMinimumSize with { X = (int)(property.value * 20f / property.max), };
	}
}
