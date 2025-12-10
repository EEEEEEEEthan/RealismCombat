using System.Collections.Generic;
public readonly struct ItemConfig
{
	public static readonly IReadOnlyDictionary<ItemIdCode, ItemConfig> configs = new Dictionary<ItemIdCode, ItemConfig>
	{
		{
			ItemIdCode.LongSword,
			new()
			{
				Name = "长剑",
				Story = "比短剑长一点",
				Flag = ItemFlagCode.Arm,
				SlotFlags = [],
				Length = 100.0,
				Weight = 1.2,
				HitPointMax = 5,
				DamageProfile = new(
					new(4f, 0f, 3f),
					new(0f, 4f, 0f),
					Damage.Zero
				),
				Protection = new(4, 4, 1),
				Coverage = 0.0,
				Icon = ResourceTable.itemIcon1Path,
			}
		},
		{
			ItemIdCode.CottonLiner,
			new()
			{
				Name = "武装衣",
				Story = "廉价的武装衣.征召兵们最常穿的装备",
				Flag = ItemFlagCode.TorsoArmor,
				SlotFlags = [ItemFlagCode.TorsoArmorMiddle,],
				Length = 60.0,
				Weight = 0.8,
				HitPointMax = 12,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(1f, 1f, 2f),
				Coverage = 0.99,
				Icon = ResourceTable.itemIcon2Path,
			}
		},
		{
			ItemIdCode.ChainMail,
			new()
			{
				Name = "链甲",
				Story = "由铁环串联而成的护甲,他们柔软但是沉重.通常需要耗费一个工匠数年时间才能完成",
				Flag = ItemFlagCode.TorsoArmorMiddle,
				SlotFlags = [ItemFlagCode.TorsoArmorOuter,],
				Length = 65.0,
				Weight = 6.5,
				HitPointMax = 18,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(3f, 1f, 0f),
				Coverage = 0.95,
				Icon = ResourceTable.itemIcon3Path,
			}
		},
		{
			ItemIdCode.PlateArmor,
			new()
			{
				Name = "板甲",
				Story = "由金属板制成的护甲,防护能力强大,但是重量也很惊人",
				Flag = ItemFlagCode.TorsoArmorOuter,
				SlotFlags = [],
				Length = 70.0,
				Weight = 12.0,
				HitPointMax = 25,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(4f, 4f, 1f),
				Coverage = 0.6,
				Icon = ResourceTable.itemIcon4Path,
			}
		},
		{
			ItemIdCode.Belt,
			new()
			{
				Name = "皮带",
				Story = "用来固定武器的皮带.上面可以挂很多武器",
				Flag = ItemFlagCode.Belt,
				SlotFlags = [ItemFlagCode.Arm, ItemFlagCode.Arm, ItemFlagCode.Arm, ItemFlagCode.Arm,],
				Length = 90.0,
				Weight = 0.5,
				HitPointMax = 8,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = Protection.Zero,
				Coverage = 0.0,
				Icon = ResourceTable.itemIcon5Path,
			}
		},
		{
			ItemIdCode.LeatherGloves,
			new()
			{
				Name = "皮手套",
				Story = "柔软的皮手套,能提供基础防护与握持摩擦",
				Flag = ItemFlagCode.HandArmor,
				SlotFlags = [],
				Length = 22.0,
				Weight = 0.25,
				HitPointMax = 8,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(1f, 1f, 1f),
				Coverage = 0.9,
				Icon = ResourceTable.itemIcon9Path,
			}
		},
		{
			ItemIdCode.ChainGloves,
			new()
			{
				Name = "链甲手套",
				Story = "由细密铁环编织而成的手套,可以防割挡刃",
				Flag = ItemFlagCode.HandArmor,
				SlotFlags = [],
				Length = 24.0,
				Weight = 0.9,
				HitPointMax = 12,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(3f, 1f, 0f),
				Coverage = 0.85,
				Icon = ResourceTable.itemIcon9Path,
			}
		},
		{
			ItemIdCode.PlateGauntlets,
			new()
			{
				Name = "板甲手套",
				Story = "包覆指节的金属手套,笨重但可靠",
				Flag = ItemFlagCode.HandArmor,
				SlotFlags = [],
				Length = 25.0,
				Weight = 2.0,
				HitPointMax = 16,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(4f, 4f, 2f),
				Coverage = 0.6,
				Icon = ResourceTable.itemIcon4Path,
			}
		},
		{
			ItemIdCode.LeatherBoots,
			new()
			{
				Name = "皮鞋",
				Story = "厚底皮鞋,适合长途行军",
				Flag = ItemFlagCode.FootArmor,
				SlotFlags = [],
				Length = 28.0,
				Weight = 0.7,
				HitPointMax = 10,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(1f, 1f, 1f),
				Coverage = 0.9,
				Icon = ResourceTable.itemIcon6Path,
			}
		},
		{
			ItemIdCode.ChainBoots,
			new()
			{
				Name = "链甲鞋",
				Story = "在皮鞋外罩上铁环,提升防护但更沉重",
				Flag = ItemFlagCode.FootArmor,
				SlotFlags = [],
				Length = 30.0,
				Weight = 2.8,
				HitPointMax = 14,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(3f, 1f, 0f),
				Coverage = 0.85,
				Icon = ResourceTable.itemIcon7Path,
			}
		},
		{
			ItemIdCode.PlateBoots,
			new()
			{
				Name = "板甲鞋",
				Story = "包覆脚背和小腿下端的板甲鞋,防护极佳",
				Flag = ItemFlagCode.FootArmor,
				SlotFlags = [],
				Length = 32.0,
				Weight = 4.5,
				HitPointMax = 20,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(4f, 4f, 2f),
				Coverage = 0.65,
				Icon = ResourceTable.itemIcon8Path,
			}
		},
		{
			ItemIdCode.Headscarf,
			new()
			{
				Name = "头巾",
				Story = "亚麻头巾,防汗也能垫在头盔下",
				Flag = ItemFlagCode.HeadArmor,
				SlotFlags = [ItemFlagCode.HeadArmorMiddle,],
				Length = 25.0,
				Weight = 0.2,
				HitPointMax = 6,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(1f, 1f, 1f),
				Coverage = 0.95,
				Icon = ResourceTable.itemIcon2Path,
			}
		},
		{
			ItemIdCode.ChainCoif,
			new()
			{
				Name = "链甲头套",
				Story = "细密铁环编织的头套,覆盖头颈部",
				Flag = ItemFlagCode.HeadArmorMiddle,
				SlotFlags = [ItemFlagCode.HeadArmorOuter,],
				Length = 28.0,
				Weight = 2.2,
				HitPointMax = 12,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(3f, 1f, 0f),
				Coverage = 0.9,
				Icon = ResourceTable.itemIcon3Path,
			}
		},
		{
			ItemIdCode.PotHelm,
			new()
			{
				Name = "罐头盔",
				Story = "简单的圆顶盔,像罐头一样把头罩住",
				Flag = ItemFlagCode.HeadArmorOuter,
				SlotFlags = [],
				Length = 30.0,
				Weight = 4.0,
				HitPointMax = 16,
				DamageProfile = new(Damage.Zero, Damage.Zero, Damage.Zero),
				Protection = new(4f, 4f, 2f),
				Coverage = 0.65,
				Icon = ResourceTable.itemIcon4Path,
			}
		},
		{
			ItemIdCode.Dagger,
			new()
			{
				Name = "匕首",
				Story = "短而轻便的匕首,容易隐藏,近身时致命",
				Flag = ItemFlagCode.Arm,
				SlotFlags = [],
				Length = 40.0,
				Weight = 0.5,
				HitPointMax = 4,
				DamageProfile = new(
					new(2f, 1f, 1f),
					new(1f, 3f, 0f),
					Damage.Zero
				),
				Protection = new(2f, 2f, 1f),
				Coverage = 0.0,
				Icon = ResourceTable.itemIcon10Path,
			}
		},
		{
			ItemIdCode.Mace,
			new()
			{
				Name = "钉头锤",
				Story = "沉重的钉头锤,用来砸碎盔甲",
				Flag = ItemFlagCode.Arm,
				SlotFlags = [],
				Length = 70.0,
				Weight = 1.6,
				HitPointMax = 6,
				DamageProfile = new(
					new(0f, 0f, 5f),
					new(0f, 1f, 4f),
					Damage.Zero
				),
				Protection = new(3f, 2f, 3f),
				Coverage = 0.0,
				Icon = ResourceTable.itemIcon1Path,
			}
		},
	};
	public string Icon { get; private init; }
	public string Name { get; private init; }
	public string? Story { get; private init; }
	public ItemFlagCode Flag { get; private init; }
	public ItemFlagCode[]? SlotFlags { get; private init; }
	public double Length { get; private init; }
	public double Weight { get; private init; }
	public int HitPointMax { get; private init; }
	public DamageProfile DamageProfile { get; private init; }
	public Protection Protection { get; private init; }
	public double Coverage { get; private init; }
}
