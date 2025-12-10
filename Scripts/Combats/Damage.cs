using System;
/// <summary>
///     伤害数值，分为劈砍/穿刺/钝击
/// </summary>
public readonly struct Damage(float slash, float pierce, float blunt)
{
	public static Damage Zero => new(0f, 0f, 0f);
	public static Damage operator +(Damage left, Damage right) => new(left.Slash + right.Slash, left.Pierce + right.Pierce, left.Blunt + right.Blunt);
	public static Damage operator -(Damage left, Protection protection) =>
		new(
			Math.Max(0f, left.Slash - protection.Slash),
			Math.Max(0f, left.Pierce - protection.Pierce),
			Math.Max(0f, left.Blunt - protection.Blunt)
		);
	public float Slash { get; } = slash;
	public float Pierce { get; } = pierce;
	public float Blunt { get; } = blunt;
	public float Total => Slash + Pierce + Blunt;
	public bool IsZero => Slash <= 0f && Pierce <= 0f && Blunt <= 0f;
}
/// <summary>
///     防护数值，分为劈砍/穿刺/钝击
/// </summary>
public readonly struct Protection(float slash, float pierce, float blunt)
{
	public static Protection Zero => new(0f, 0f, 0f);
	public float Slash { get; } = slash;
	public float Pierce { get; } = pierce;
	public float Blunt { get; } = blunt;
	public Protection Add(Protection other) => new(Slash + other.Slash, Pierce + other.Pierce, Blunt + other.Blunt);
}
/// <summary>
///     不同攻击类别对应的基础伤害表
/// </summary>
public readonly struct DamageProfile(Damage swing, Damage thrust, Damage special)
{
	public Damage Swing { get; } = swing;
	public Damage Thrust { get; } = thrust;
	public Damage Special { get; } = special;
	public Damage Get(AttackTypeCode type) =>
		type switch
		{
			AttackTypeCode.Swing => Swing,
			AttackTypeCode.Thrust => Thrust,
			_ => Special,
		};
}
