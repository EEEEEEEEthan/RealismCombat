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
				_ => @this.ToString(),
			};
	}
}
/// <summary>
///     Buff类
/// </summary>
public class Buff(BuffCode code, Character? source = null)
{
	public readonly BuffCode code = code;
	/// <summary>
	///     Buff的来源角色
	/// </summary>
	public readonly Character? source = source;
}
