using Godot;
namespace RealismCombat.Extensions;
public static class GodotObjectExtensions
{
	public static bool Valid(this GodotObject obj) => GodotObject.IsInstanceValid(obj);
}
