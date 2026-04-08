extends Node
class_name Character

@export var character_name: String
@onready var head: BodyPart = %Head
@onready var chest: BodyPart = %Chest
@onready var right_hand: BodyPart = %RightHand
@onready var left_hand: BodyPart = %LeftHand
@onready var right_foot: BodyPart = %RightFoot
@onready var left_foot: BodyPart = %LeftFoot
@onready var game: Game = get_parent()
@onready var all_body_parts: Array[BodyPart] = [
	head,
	chest,
	right_hand,
	left_hand,
	right_foot,
	left_foot,
]
var state_machine: CharacterStateMachine

var actions: Array[Action]:
	get:
		if not actions:
			actions = []
			actions.append_array(%Actions.get_children())
		return actions

var alive: bool:
	get:
		return head.hp.value > 0 and chest.hp.value > 0

var speed: float:
	get:
		return 1

func add_action(action: Action) -> void:
	%actions.add_child(action)
	actions = []

func on_enter_combat() -> void:
	state_machine = %CharacterStateMachineTemplate.create_instance()

func on_exit_combat() -> void:
	state_machine.queue_free()
	state_machine = null
