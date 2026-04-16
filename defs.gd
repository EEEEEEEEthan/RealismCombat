@tool
class_name Defs

enum BodyPart
{
	HEAD,
	CHEST,
	RIGHT_HAND,
	LEFT_HAND,
	RIGHT_FOOT,
	LEFT_FOOT,
}

## 四档色阶的色系；成员顺序须与 _FAMILY_COLORS 每连续四项（一族）一致。NEUTRAL 为黑与三级灰。
enum ColorFamily
{
	NEUTRAL,
	SLATE_CYAN,
	PINE_MINT,
	LEAF_GREEN,
	SPRING_LIME,
	FIELD_GOLD,
	TORCH_AMBER,
	FORGE_EMBER,
	ROSE_PINK,
	WILD_MAGENTA,
	TWILIGHT_VIOLET,
	SAPPHIRE,
	OCEAN_BLUE,
}

enum ColorShade
{
	DARK,
	MID,
	BRIGHT,
	LIGHT,
}

static func get_body_part_name(part: Defs.BodyPart) -> StringName:
	match part:
		Defs.BodyPart.HEAD:
			return &"头部"
		Defs.BodyPart.CHEST:
			return &"胸部"
		Defs.BodyPart.RIGHT_HAND:
			return &"右手"
		Defs.BodyPart.LEFT_HAND:
			return &"左手"
		Defs.BodyPart.RIGHT_FOOT:
			return &"右脚"
		Defs.BodyPart.LEFT_FOOT:
			return &"左脚"
	return &"Unknown"


const _SHADES_PER_FAMILY: int = 4 ## 须与 ColorShade 档位数一致

## 一维：每连续四项为一族，顺序同 ColorFamily；族内四色顺序同 ColorShade。
static var _FAMILY_COLORS: PackedColorArray = PackedColorArray([
	Color("000000"), Color("797979"), Color("a2a2a2"), Color("ebebeb"),
	Color("305182"), Color("4192c3"), Color("61d3e3"), Color("a2fff3"),
	Color("306141"), Color("49a269"), Color("71e392"), Color("a2ffcb"),
	Color("386d00"), Color("49aa10"), Color("71f341"), Color("a2f3a2"),
	Color("386900"), Color("51a200"), Color("9aeb00"), Color("cbf382"),
	Color("495900"), Color("8a8a00"), Color("ebd320"), Color("fff392"),
	Color("794100"), Color("c37100"), Color("ffa200"), Color("ffdba2"),
	Color("a23000"), Color("e35100"), Color("ff7930"), Color("ffcbaa"),
	Color("b21030"), Color("db4161"), Color("ff61b2"), Color("ffbaeb"),
	Color("9a2079"), Color("db41c3"), Color("f361ff"), Color("e3b2ff"),
	Color("6110a2"), Color("9241f3"), Color("a271ff"), Color("c3b2ff"),
	Color("2800ba"), Color("4141ff"), Color("5182ff"), Color("a2baff"),
	Color("2000b2"), Color("4161fb"), Color("61a2ff"), Color("92d3ff"),
])

static func get_family_color(family: ColorFamily, shade: ColorShade) -> Color:
	return _FAMILY_COLORS[(family as int) * _SHADES_PER_FAMILY + (shade as int)]
