extends RefCounted
class_name CharacterStateMachine

const _IDLE_STATE_SCRIPT := "res://game/combat/character_state_machine_idle_state.gd"
const _ACTION_STATE_SCRIPT := "res://game/combat/character_state_machine_action_state.gd"

var action_points := Property.new(0, 10)
var character: Character
var idle_state
var action_state
var current_state

var game: Game:
	get:
		return character.game

var combat: Combat:
	get:
		return game.combat

var character_renderer: CharacterRenderer:
	get:
		return combat.get_character_renderer(character)

var is_player: bool:
	get:
		return combat.is_player_character(character)

var action_points_per_tick: float:
	get:
		return character.speed * 0.05


func _init(p_character: Character) -> void:
	character = p_character
	var idle_cls = load(_IDLE_STATE_SCRIPT) as GDScript
	var action_cls = load(_ACTION_STATE_SCRIPT) as GDScript
	idle_state = idle_cls.new(self)
	action_state = action_cls.new(self)
	current_state = idle_state


func new_tick() -> void:
	await current_state.new_tick()


func _set_action(action: Action, from_body: BodyPart, to_body: BodyPart) -> void:
	current_state = action_state
	await action_state.set_action(action, from_body, to_body)


func _set_idle(points: float) -> void:
	current_state = idle_state
	action_points.value -= points
