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

# 305182
# 4192c3
# 61d3e3
# a2fff3

# 306141
# 49a269
# 71e392
# a2ffcb

# 386d00
# 49aa10
# 71f341
# a2f3a2

# 386900
# 51a200
# 9aeb00
# cbf382

# 495900
# 8a8a00
# ebd320
# fff392

# 794100
# c37100
# ffa200
# ffdba2

# a23000
# e35100
# ff7930
# ffcbba

static var COLOR_DARK_PINK: Color = Color("b21030")
# db4161
# ff61b2
# ffbaeb

# 9a2079
# db41c3
# f361ff
# e3b2ff

# 6110a2
# 9241f3
# a271ff
# c3b2ff

static var COLOR_NAVI_BLUE: Color = Color("2800ba")
# 4141ff
# 5182ff
static var COLOR_LIGHT_BLUE: Color = Color("a2baff")

# 2000b2
# 4161fb
# 61a2ff
# 92d3ff
