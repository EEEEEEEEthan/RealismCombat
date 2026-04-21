@abstract
extends RefCounted
class_name Action

var character: Character

func _init(chr: Character) -> void:
	character = chr

func prepare(from_body: BodyPart, _to_body: BodyPart) -> void:
	var menu = Dialogues.create_generic_dialogue()
	menu.text = from_body.character.character_name + "的" + from_body.part_name() + "开始蓄力..."
	await menu.pressed
	menu.queue_free()

func get_execution_text(from_body: BodyPart, to_body: BodyPart) -> String:
	return from_body.character.character_name + "的" + from_body.part_name() + "对" + to_body.character.character_name + "的" + to_body.part_name() + "发动" + get_name()

func execute(from_body: BodyPart, to_body: BodyPart) -> GenericDialogue:
	var dialogue = Dialogues.create_generic_dialogue()
	dialogue.text = get_execution_text(from_body, to_body)
	await dialogue.pressed
	return dialogue

func preview(_from_body: BodyPart, _to_body: BodyPart) -> String:
	push_error("override me")
	return "暂无预览"

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

func get_description() -> String:
	return &"前后摇:" + str(get_windup_action_points()) + &"/" + str(get_recovery_action_points())

func get_name() -> StringName:
	return &"unknown"

func get_windup_action_points() -> int:
	return 0

func get_recovery_action_points() -> int:
	return 0
