using Godot;
namespace RealismCombat;
static class GameColors
{
	public static readonly Color normalControl = Colors.White;
	public static readonly Color activeControl = new("#e35100ff");
	public static readonly Color hurtFlash = new(178f / 255f, 16f / 255f, 48f / 255f, 1f);
	public static readonly Color inactiveControl = new(178f / 255f, 178f / 255f, 178f / 255f, 1f);
}
