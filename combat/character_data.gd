extends Node
class_name CharacterData

@export var character_name: String
var head: BodyPartData
var chest: BodyPartData
var right_hand: BodyPartData
var left_hand: BodyPartData
var right_foot: BodyPartData
var left_foot: BodyPartData

func _init() -> void:
	head = BodyPartData.new()
	chest = BodyPartData.new()
	right_hand = BodyPartData.new()
	left_hand = BodyPartData.new()
	right_foot = BodyPartData.new()
	left_foot = BodyPartData.new()
	head.part = Defs.BodyPart.HEAD
