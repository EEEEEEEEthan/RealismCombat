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

static var COLOR_DARK_PINK: Color = Color("b21030")
