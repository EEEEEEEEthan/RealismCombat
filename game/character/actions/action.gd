@abstract
extends Node
class_name Action

@onready var character: Character = get_parent().get_parent()
var action_name: StringName:
	get: return _get_name()

var description: String:
	get: return _get_description()

func static_valid_from_body(_from_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func dynamic_valid_from_body(_from_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func static_valid_to_body(_to_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func dynamic_valid_to_body(_to_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func _get_description() -> String:
	return &"unknown"

func _get_name() -> StringName:
	return &"unknown"
