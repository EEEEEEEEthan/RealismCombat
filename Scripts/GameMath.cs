using System;
/// <summary>
///     通用数学工具
/// </summary>
public static class GameMath
{
	/// <summary>
	///     将数值映射到 [-2,2] 近似区间
	/// </summary>
	public static double ScaleToRange(double value, double scale) => 2.0 * Math.Tanh(value / scale);
	/// <summary>
	///     计算 Sigmoid
	/// </summary>
	public static double Sigmoid(double value) => 1.0 / (1.0 + Math.Exp(-value));
}
