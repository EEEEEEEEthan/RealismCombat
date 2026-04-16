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

static var COLOR_DARK_GREY: Color = Color("797979")
static var COLOR_MID_GREY: Color = Color("a2a2a2")
static var COLOR_LIGHT_GREY: Color = Color("ebebeb")

static var COLOR_SLATE_CYAN_DARK: Color = Color("305182")
static var COLOR_SLATE_CYAN_MID: Color = Color("4192c3")
static var COLOR_SLATE_CYAN_BRIGHT: Color = Color("61d3e3")
static var COLOR_SLATE_CYAN_LIGHT: Color = Color("a2fff3")

static var COLOR_PINE_MINT_DARK: Color = Color("306141")
static var COLOR_PINE_MINT_MID: Color = Color("49a269")
static var COLOR_PINE_MINT_BRIGHT: Color = Color("71e392")
static var COLOR_PINE_MINT_LIGHT: Color = Color("a2ffcb")

static var COLOR_LEAF_GREEN_DARK: Color = Color("386d00")
static var COLOR_LEAF_GREEN_MID: Color = Color("49aa10")
static var COLOR_LEAF_GREEN_BRIGHT: Color = Color("71f341")
static var COLOR_LEAF_GREEN_LIGHT: Color = Color("a2f3a2")

static var COLOR_SPRING_LIME_DARK: Color = Color("386900")
static var COLOR_SPRING_LIME_MID: Color = Color("51a200")
static var COLOR_SPRING_LIME_BRIGHT: Color = Color("9aeb00")
static var COLOR_SPRING_LIME_LIGHT: Color = Color("cbf382")

static var COLOR_FIELD_GOLD_DARK: Color = Color("495900")
static var COLOR_FIELD_GOLD_MID: Color = Color("8a8a00")
static var COLOR_FIELD_GOLD_BRIGHT: Color = Color("ebd320")
static var COLOR_FIELD_GOLD_LIGHT: Color = Color("fff392")

static var COLOR_TORCH_AMBER_DARK: Color = Color("794100")
static var COLOR_TORCH_AMBER_MID: Color = Color("c37100")
static var COLOR_TORCH_AMBER_BRIGHT: Color = Color("ffa200")
static var COLOR_TORCH_AMBER_LIGHT: Color = Color("ffdba2")

static var COLOR_FORGE_EMBER_DARK: Color = Color("a23000")
static var COLOR_FORGE_EMBER_MID: Color = Color("e35100")
static var COLOR_FORGE_EMBER_BRIGHT: Color = Color("ff7930")
static var COLOR_FORGE_EMBER_LIGHT: Color = Color("ffcbaa")

static var COLOR_DARK_PINK: Color = Color("b21030")
static var COLOR_ROSE_PINK_MID: Color = Color("db4161")
static var COLOR_ROSE_PINK_BRIGHT: Color = Color("ff61b2")
static var COLOR_ROSE_PINK_LIGHT: Color = Color("ffbaeb")

static var COLOR_WILD_MAGENTA_DARK: Color = Color("9a2079")
static var COLOR_WILD_MAGENTA_MID: Color = Color("db41c3")
static var COLOR_WILD_MAGENTA_BRIGHT: Color = Color("f361ff")
static var COLOR_WILD_MAGENTA_LIGHT: Color = Color("e3b2ff")

static var COLOR_TWILIGHT_VIOLET_DARK: Color = Color("6110a2")
static var COLOR_TWILIGHT_VIOLET_MID: Color = Color("9241f3")
static var COLOR_TWILIGHT_VIOLET_BRIGHT: Color = Color("a271ff")
static var COLOR_TWILIGHT_VIOLET_LIGHT: Color = Color("c3b2ff")

static var COLOR_SAPPHIRE_DARK: Color = Color("2800ba")
static var COLOR_SAPPHIRE_MID: Color = Color("4141ff")
static var COLOR_SAPPHIRE_BRIGHT: Color = Color("5182ff")
static var COLOR_SAPPHIRE_LIGHT: Color = Color("a2baff")

static var COLOR_OCEAN_BLUE_DARK: Color = Color("2000b2")
static var COLOR_OCEAN_BLUE_MID: Color = Color("4161fb")
static var COLOR_OCEAN_BLUE_BRIGHT: Color = Color("61a2ff")
static var COLOR_OCEAN_BLUE_ICE: Color = Color("92d3ff")
