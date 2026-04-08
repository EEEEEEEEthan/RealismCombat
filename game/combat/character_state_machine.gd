extends Node
class_name CharacterStateMachine

@onready var character: Character = get_parent()
var action_points:= Property.new(0, 10)

@onready var current_state: Node = %IdleState

func new_tick() -> void:
	await current_state.new_tick()
