using Godot;
namespace RealismCombat.Nodes;
public partial class Character : Node
{
	public readonly PropertyInt hp;
	public readonly PropertyInt speed;
	public readonly PropertySingle actionPoint;
	public readonly string name;
	public Character(string name)
	{
		hp = new() { maxValue = 10, value = 10, };
		speed = new() { maxValue = 10, value = 10, };
		actionPoint = new() { maxValue = 0, value = 0, };
		this.name = name;
	}
}
