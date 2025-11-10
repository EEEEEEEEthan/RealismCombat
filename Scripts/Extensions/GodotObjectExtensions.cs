using Godot;
namespace RealismCombat.Extensions;
public static partial class Extensions
{
	public static bool Valid(this GodotObject obj) => GodotObject.IsInstanceValid(obj);
}
