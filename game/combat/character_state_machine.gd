extends Node
class_name CharacterStateMachine

@onready var character: Character = get_parent()
var action_points:= Property.new(0, 10)

func new_tick() -> void:
	pass
