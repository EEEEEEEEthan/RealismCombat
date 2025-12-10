/// <summary>
///     Buff类型
/// </summary>
public enum BuffCode
{
	/// <summary>
	///     束缚
	/// </summary>
	Restrained,
	/// <summary>
	///     擒拿
	/// </summary>
	Grappling,
	/// <summary>
	///     流血
	/// </summary>
	Bleeding,
	/// <summary>
	///     倒伏
	/// </summary>
	Prone,
	/// <summary>
	///     招架
	/// </summary>
	Parrying,
}
/// <summary>
///     Buff类型扩展工具
/// </summary>
public static class BuffCodeExtensions
{
	extension(BuffCode @this)
	{
		public string Name =>
			@this switch
			{
				BuffCode.Restrained => "束缚",
				BuffCode.Grappling => "擒拿",
				BuffCode.Bleeding => "流血",
				BuffCode.Prone => "倒伏",
				BuffCode.Parrying => "招架",
				_ => @this.ToString(),
			};
	}
}
/// <summary>
///     Buff来源信息，记录施加者以及对应的战斗目标
/// </summary>
public readonly record struct BuffSource(Character Character, ICombatTarget Target);
/// <summary>
///     Buff类
/// </summary>
public class Buff(BuffCode code, BuffSource? source = null)
{
	public readonly BuffCode code = code;
	/// <summary>
	///     Buff的来源信息
	/// </summary>
	public readonly BuffSource? source = source;
}
