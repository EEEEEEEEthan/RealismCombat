using System;
using System.Collections.Generic;
using Godot;
public readonly struct ItemConfig
{
	public static readonly IReadOnlyDictionary<ItemIdCode, ItemConfig> configs = new Dictionary<ItemIdCode, ItemConfig>
	{
		{
			ItemIdCode.LongSword,
			new()
			{
				Name = "长剑",
				Description = "比短剑长一点",
				Flag = ItemFlagCode.Arm,
				SlotFlags = [],
				Length = 100.0,
				Weight = 1.2,
				HitPointMax = 10,
				DamageProfile = new(
					new(4f, 0f, 3f),
					new(0f, 4f, 0f),
					Damage.Zero
				),
				Protection = Protection.Zero,
				IconGetter = () => SpriteTable.LongSword,
			}
		},
		{
			ItemIdCode.CottonLiner,
			new()
			{
				Name = "绵甲",
				Description = "廉价的武装衣.征召兵们最常穿的装备",
				Flag = ItemFlagCode.TorsoArmor,
				SlotFlags = [],
				Length = 60.0,
				Weight = 0.8,
				HitPointMax = 12,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(1f, 1f, 1f),
				IconGetter = () => SpriteTable.CottonLiner,
			}
		},
		{
			ItemIdCode.ChainMail,
			new()
			{
				Name = "链甲",
				Description = "由铁环串联而成的护甲,他们柔软但是沉重.通常需要耗费一个工匠数年时间才能完成",
				Flag = ItemFlagCode.TorsoArmor,
				SlotFlags = [],
				Length = 65.0,
				Weight = 6.5,
				HitPointMax = 18,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(4f, 2f, 1f),
				IconGetter = () => SpriteTable.ChainMail,
			}
		},
		{
			ItemIdCode.PlateArmor,
			new()
			{
				Name = "板甲",
				Description = "由金属板制成的护甲,防护能力强大,但是重量也很惊人",
				Flag = ItemFlagCode.TorsoArmor,
				SlotFlags = [],
				Length = 70.0,
				Weight = 12.0,
				HitPointMax = 25,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(4f, 4f, 2f),
				IconGetter = () => SpriteTable.PlateArmor,
			}
		},
		{
			ItemIdCode.Belt,
			new()
			{
				Name = "皮带",
				Description = "用来固定武器的皮带.上面可以挂很多武器",
				Flag = ItemFlagCode.Belt,
				SlotFlags = [ItemFlagCode.Arm, ItemFlagCode.Arm, ItemFlagCode.Arm, ItemFlagCode.Arm,],
				Length = 90.0,
				Weight = 0.5,
				HitPointMax = 8,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = Protection.Zero,
				IconGetter = () => SpriteTable.Belt,
			}
		},
		{
			ItemIdCode.CottonPants,
			new()
			{
				Name = "棉裤",
				Description = "填充棉絮的护腿,保暖又廉价",
				Flag = ItemFlagCode.LegArmor,
				SlotFlags = [],
				Length = 90.0,
				Weight = 0.6,
				HitPointMax = 8,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(1f, 1f, 1f),
				IconGetter = () => SpriteTable.CottonPants,
			}
		},
		{
			ItemIdCode.ChainChausses,
			new()
			{
				Name = "链甲护腿",
				Description = "由细密铁环编织的护腿,沉重但可靠",
				Flag = ItemFlagCode.LegArmor,
				SlotFlags = [],
				Length = 95.0,
				Weight = 4.5,
				HitPointMax = 14,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(4f, 2f, 1f),
				IconGetter = () => SpriteTable.ChainChausses,
			}
		},
		{
			ItemIdCode.PlateGreaves,
			new()
			{
				Name = "板甲护腿",
				Description = "覆盖小腿的金属板,能挡住大部分攻击",
				Flag = ItemFlagCode.LegArmor,
				SlotFlags = [],
				Length = 95.0,
				Weight = 7.5,
				HitPointMax = 20,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(4f, 4f, 2f),
				IconGetter = () => SpriteTable.PlateGreaves,
			}
		},
	};
	public Texture2D Icon => IconGetter();
	public string Name { get; private init; }
	public string? Description { get; private init; }
	public ItemFlagCode Flag { get; private init; }
	public ItemFlagCode[]? SlotFlags { get; private init; }
	public double Length { get; private init; }
	public double Weight { get; private init; }
	public int HitPointMax { get; private init; }
	public DamageProfile DamageProfile { get; private init; }
	public Protection Protection { get; private init; }
	Func<Texture2D> IconGetter { get; init; }
}
