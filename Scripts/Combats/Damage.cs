using System;

/// <summary>
///     伤害数值，分为劈砍/穿刺/钝击
/// </summary>
public readonly struct Damage
{
	public Damage(float slash, float pierce, float blunt)
	{
		this.slash = slash;
		this.pierce = pierce;
		this.blunt = blunt;
	}
	public float slash { get; }
	public float pierce { get; }
	public float blunt { get; }
	public static Damage Zero => new(0f, 0f, 0f);
	public float Total => slash + pierce + blunt;
	public bool IsZero => slash <= 0f && pierce <= 0f && blunt <= 0f;
	public Damage Scale(double factor)
	{
		var f = (float)factor;
		return new Damage(slash * f, pierce * f, blunt * f);
	}
	public Damage ApplyProtection(Protection protection) =>
		new(
			Math.Max(0f, slash - protection.slash),
			Math.Max(0f, pierce - protection.pierce),
			Math.Max(0f, blunt - protection.blunt)
		);
	public static Damage operator +(Damage left, Damage right) =>
		new(left.slash + right.slash, left.pierce + right.pierce, left.blunt + right.blunt);
}

/// <summary>
///     防护数值，分为劈砍/穿刺/钝击
/// </summary>
public readonly struct Protection
{
	public Protection(float slash, float pierce, float blunt)
	{
		this.slash = slash;
		this.pierce = pierce;
		this.blunt = blunt;
	}
	public float slash { get; }
	public float pierce { get; }
	public float blunt { get; }
	public static Protection Zero => new(0f, 0f, 0f);
	public Protection Add(Protection other) =>
		new(slash + other.slash, pierce + other.pierce, blunt + other.blunt);
}

/// <summary>
///     不同攻击类别对应的基础伤害表
/// </summary>
public readonly struct DamageProfile
{
	public DamageProfile(Damage swing, Damage thrust, Damage special)
	{
		Swing = swing;
		Thrust = thrust;
		Special = special;
	}
	public Damage Swing { get; }
	public Damage Thrust { get; }
	public Damage Special { get; }
	public Damage Get(AttackTypeCode type) =>
		type switch
		{
			AttackTypeCode.Swing => Swing,
			AttackTypeCode.Thrust => Thrust,
			_ => Special,
		};
}

