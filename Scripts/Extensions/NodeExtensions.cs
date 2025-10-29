using Godot;
namespace RealismCombat.Extensions;
static class NodeExtensions
{
	public static void DestroyChildren(this Node node)
	{
		foreach (var child in node.GetChildren()) child.QueueFree();
	}
}
