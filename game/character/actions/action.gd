@abstract
extends Node
class_name Action

@onready var character: Character = get_parent().get_parent()
var action_name: StringName:
	get: return _get_name()

var description: String:
	get: return _get_description()

var windup_action_points: int:
	get: return _get_windup_action_points()

var recovery_action_points: int:
	get: return _get_recovery_action_points()

func static_valid_from_body(_from_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func dynamic_valid_from_body(_from_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func static_valid_to_character(_to_character: Character) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func dynamic_valid_to_character(_to_character: Character) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func static_valid_to_body(_to_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func dynamic_valid_to_body(_to_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func _get_description() -> String:
	return &"unknown"

func _get_name() -> StringName:
	return &"unknown"

func _get_windup_action_points() -> int:
	return 0

func _get_recovery_action_points() -> int:
	return 0
