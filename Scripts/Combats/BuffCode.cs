namespace RealismCombat.Combats;
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
	/// <summary>
	///     获取Buff类型的显示名称
	/// </summary>
	public static string GetName(this BuffCode @this) =>
		@this switch
		{
			BuffCode.Restrained => "束缚",
			BuffCode.Grappling => "擒拿",
			_ => @this.ToString(),
		};
}

