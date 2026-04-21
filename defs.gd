@tool
class_name Defs

enum Side
{
	LEFT,
	RIGHT,
}

## 四档色阶的色系；成员顺序须与 _FAMILY_COLORS 每连续四项（一族）一致。NEUTRAL 为黑与三级灰。
enum ColorFamily
{
	## 无覆盖等占位，禁止传入 get_family_color
	NONE = -1,
	NEUTRAL = 0,
	SLATE_CYAN = 1,
	PINE_MINT = 2,
	LEAF_GREEN = 3,
	SPRING_LIME = 4,
	FIELD_GOLD = 5,
	TORCH_AMBER = 6,
	FORGE_EMBER = 7,
	ROSE_PINK = 8,
	WILD_MAGENTA = 9,
	TWILIGHT_VIOLET = 10,
	SAPPHIRE = 11,
	OCEAN_BLUE = 12,
}

enum ColorShade
{
	DARK,
	MID,
	BRIGHT,
	LIGHT,
}

const _SHADES_PER_FAMILY: int = 4 ## 须与 ColorShade 档位数一致

## 一维：每连续四项为一族，顺序同 ColorFamily；族内四色顺序同 ColorShade。
static var _FAMILY_COLORS: PackedColorArray = PackedColorArray([
	Color("000000"), Color("797979"), Color("a2a2a2"), Color("ebebeb"),  # netural
	Color("305182"), Color("4192c3"), Color("61d3e3"), Color("a2fff3"),  # slate_cyan
	Color("306141"), Color("49a269"), Color("71e392"), Color("a2ffcb"),  # pine_mint
	Color("386d00"), Color("49aa10"), Color("71f341"), Color("a2f3a2"),  # leaf_green
	Color("386900"), Color("51a200"), Color("9aeb00"), Color("cbf382"),  # spring_lime
	Color("495900"), Color("8a8a00"), Color("ebd320"), Color("fff392"),  # field_gold
	Color("794100"), Color("c37100"), Color("ffa200"), Color("ffdba2"),  # torch_amber
	Color("a23000"), Color("e35100"), Color("ff7930"), Color("ffcbaa"),  # forge_amber
	Color("b21030"), Color("db4161"), Color("ff61b2"), Color("ffbaeb"),  # ROSE_PINK
	Color("9a2079"), Color("db41c3"), Color("f361ff"), Color("e3b2ff"),  # wild_magenta
	Color("6110a2"), Color("9241f3"), Color("a271ff"), Color("c3b2ff"),  # TWILIGHT_VIOLET
	Color("2800ba"), Color("4141ff"), Color("5182ff"), Color("a2baff"),  # SAPPHIRE
	Color("2000b2"), Color("4161fb"), Color("61a2ff"), Color("92d3ff"),  # OCEAN_BLUE
])

static func get_family_color(family: ColorFamily, shade: ColorShade) -> Color:
	var fi := family as int
	assert(fi >= 0, "ColorFamily.NONE 无调色板")
	return _FAMILY_COLORS[fi * _SHADES_PER_FAMILY + (shade as int)]


## 菜单项「禁用但可见」时的整体色调（非白/黑/透明须走色阶）
static func get_menu_option_disabled_modulate() -> Color:
	return get_family_color(ColorFamily.NEUTRAL, ColorShade.MID)
