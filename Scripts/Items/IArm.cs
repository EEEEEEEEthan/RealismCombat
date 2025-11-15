namespace RealismCombat.Items;
/// <summary>
///     武器类型装备接口
/// </summary>
public interface IArm
{
	/// <summary>
	///     武器长度（单位：厘米）
	/// </summary>
	double Length { get; }
	/// <summary>
	///     武器重量（单位：千克）
	/// </summary>
	double Weight { get; }
}
