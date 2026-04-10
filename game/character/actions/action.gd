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

func prepare(from_body: BodyPart, _to_body: BodyPart) -> void:
	var menu = Dialogues.create_generic_dialogue()
	menu.text = from_body.character.character_name + "的" + from_body.part_name + "开始蓄力..."
	await menu.pressed
	menu.queue_free()

func execute(from_body: BodyPart, to_body: BodyPart) -> void:
	var menu = Dialogues.create_generic_dialogue()
	menu.text = from_body.character.character_name + "的" + from_body.part_name + "对" + to_body.character.character_name + "的" + to_body.part_name + "发动" + action_name
	await menu.pressed
	menu.queue_free()

func static_valid_from_body(_from_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func dynamic_valid_from_body(_from_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func valid_from_body(from_body: BodyPart) -> bool:
	return static_valid_from_body(from_body).success and dynamic_valid_from_body(from_body).success

func static_valid_to_character(_to_character: Character) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func dynamic_valid_to_character(_to_character: Character) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func valid_to_character(to_character: Character) -> bool:
	return static_valid_to_character(to_character).success and dynamic_valid_to_character(to_character).success

func static_valid_to_body(_to_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func dynamic_valid_to_body(_to_body: BodyPart) -> Outcome:
	return Outcome.from_failure("抽象基类禁止")

func valid_to_body(to_body: BodyPart) -> bool:
	return static_valid_to_body(to_body).success and dynamic_valid_to_body(to_body).success

func get_weight(_from_body: BodyPart, _to_body: BodyPart) -> float:
	push_error("抽象基类禁止")
	return 1

func _get_description() -> String:
	return &"unknown"

func _get_name() -> StringName:
	return &"unknown"

func _get_windup_action_points() -> int:
	return 0

func _get_recovery_action_points() -> int:
	return 0
