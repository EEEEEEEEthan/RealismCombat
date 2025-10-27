using Godot;
namespace RealismCombat.Extensions;
public static partial class Extensions
{
	public static bool Valid(this Node node) => GodotObject.IsInstanceValid(node);
}
