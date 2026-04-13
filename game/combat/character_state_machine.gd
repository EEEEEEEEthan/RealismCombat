extends Node
class_name CharacterStateMachine

var action_points := Property.new(0, 10)
@onready var current_state: Node = %IdleState
var character: Character:
	get: return get_parent()
var game: Game:
	get: return character.game
var combat: Combat:
	get: return game.combat
var is_player: bool:
	get: return combat.is_player_character(character)
var action_points_per_tick: float:
	get: return character.speed * 0.05

func new_tick() -> void:
	await current_state.new_tick()

func _set_action(action: Action, from_body: BodyPart, to_body: BodyPart) -> void:
	current_state = %ActionState
	await %ActionState.set_action(action, from_body, to_body)

func _set_idle(points: float) -> void:
	current_state = %IdleState
	action_points.value -= points
