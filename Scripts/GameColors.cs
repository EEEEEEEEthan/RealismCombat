using Godot;
namespace RealismCombat;
static class GameColors
{
	public static readonly Color normalControl = Colors.White;
	public static readonly Color activeControl = new("#e35100ff");
	public static readonly Color hurtFlash = new(r: 178f / 255f, g: 16f / 255f, b: 48f / 255f, a: 1f);
	public static readonly Color inactiveControl = new(r: 178f / 255f, g: 178f / 255f, b: 178f / 255f, a: 1f);
}
