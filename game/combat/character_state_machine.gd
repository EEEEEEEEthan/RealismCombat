extends Node
class_name CharacterStateMachine

var character: Character:
	get: return get_parent()
var game: Game:
	get: return character.game
var combat: Combat:
	get: return game.combat
var is_player: bool:
	get: return combat.characters[character] == 0
var action_points:= Property.new(0, 10)

@onready var current_state: Node = %IdleState

func new_tick() -> void:
	await current_state.new_tick()

func _set_action(action:Action, from_body:BodyPart, to_body:BodyPart) -> void:
	current_state = %ActionState
	await %ActionState.set_action(action, from_body, to_body)
