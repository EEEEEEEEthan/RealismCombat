extends Node
class_name Character

var character_name: String
@onready var head: BodyPart = %Head
@onready var chest: BodyPart = %Chest
@onready var right_hand: BodyPart = %RightHand
@onready var left_hand: BodyPart = %LeftHand
@onready var right_foot: BodyPart = %RightFoot
@onready var left_foot: BodyPart = %LeftFoot
var all_body_parts: Array[BodyPart]
var action_points:= Property.new(0, 10)

var alive: bool:
	get:
		return head.hp.value > 0 and chest.hp.value > 0

var speed: float:
	get:
		return 1

func _ready() -> void:
	all_body_parts.append(head)
	all_body_parts.append(chest)
	all_body_parts.append(right_hand)
	all_body_parts.append(left_hand)
	all_body_parts.append(right_foot)
	all_body_parts.append(left_foot)
