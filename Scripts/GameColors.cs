using System.Collections.Generic;
using Godot;
namespace RealismCombat;
/// <summary>
///     定义游戏中会复用的颜色。以下gradient都是从浅到深排列
/// </summary>
public static class GameColors
{
	public static readonly Color transparent = Colors.Transparent;
	public static readonly IReadOnlyList<Color>
		grayGradient = [new("#ffffff"), new("#ebebeb"), new("#b2b2b2"), new("#a2a2a2"), new("#797979"), new("#000000"),];
	public static readonly IReadOnlyList<Color> skyBlueGradient = [new("#a2baff"), new("#5182ff"), new("#4141ff"), new("#2800ba"),];
	public static readonly IReadOnlyList<Color> sunFlareOrangeGradient = [new("#ffcbba"), new("#ff7930"), new("#e35100"), new("#e23000"),];
	public static readonly IReadOnlyList<Color> pinkGradient = [new("#ffbaeb"), new("#ff61b2"), new("#db4161"), new("#b21030"),];
}
