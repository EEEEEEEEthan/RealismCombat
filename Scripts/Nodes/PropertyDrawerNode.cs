using System.Text;
using Godot;
namespace RealismCombat.Nodes;
partial class PropertyDrawerNode : Node
{
	public (int value, int max) property = (10, 10);
	public string title;
	[Export] Label name = null!;
	[Export] Label value = null!;
	public override void _Process(double delta)
	{
		name.Text = title;
		var builder = new StringBuilder();
		for (var i = 0; i < property.value; i++) builder.Append("▮");
		for (var i = property.value; i < property.max; i++) builder.Append("▯");
		value.Text = builder.ToString();
	}
}
