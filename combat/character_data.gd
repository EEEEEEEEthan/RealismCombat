class_name Character

@export var character_name: String
var head: BodyPartData
var chest: BodyPartData
var right_hand: BodyPartData
var left_hand: BodyPartData
var right_foot: BodyPartData
var left_foot: BodyPartData
var all_body_parts: Array[BodyPartData]

func _init() -> void:
	head = BodyPartData.new()
	chest = BodyPartData.new()
	right_hand = BodyPartData.new()
	left_hand = BodyPartData.new()
	right_foot = BodyPartData.new()
	left_foot = BodyPartData.new()
	head.part = Defs.BodyPart.HEAD
	chest.part = Defs.BodyPart.CHEST
	right_hand.part = Defs.BodyPart.RIGHT_HAND
	left_hand.part = Defs.BodyPart.LEFT_HAND
	right_foot.part = Defs.BodyPart.RIGHT_FOOT
	left_foot.part = Defs.BodyPart.LEFT_FOOT
	all_body_parts.append(head)
	all_body_parts.append(chest)
	all_body_parts.append(right_hand)
	all_body_parts.append(left_hand)
	all_body_parts.append(right_foot)
	all_body_parts.append(left_foot)
