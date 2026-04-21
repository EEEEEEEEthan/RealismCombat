extends RefCounted
class_name CharacterStateMachine

## 每 tick 行动力 = speed * 此系数；与攻击前摇折算 tick 共用
const ACTION_POINTS_PER_SPEED_UNIT_PER_TICK := 0.05

var action_points := Property.new(0, 10)
var combat_character: CombatCharacter
var idle_state
var current_state

var character: Character:
	get: return combat_character.raw_character

var game: Game:
	get: return character.game

var combat: Combat:
	get: return game.combat

var character_renderer: CharacterRenderer:
	get: return combat.get_character_renderer(combat_character)

var is_player: bool:
	get: return combat.is_player_character(combat_character)

var action_points_per_tick: float:
	get: return character.speed * ACTION_POINTS_PER_SPEED_UNIT_PER_TICK

var remaining_windup_ticks: int:
	get:
		if not (current_state is CharacterStateMachineActionState):
			return 0
		return (current_state as CharacterStateMachineActionState).remaining_windup_ticks

func _init(p_combat_character: CombatCharacter) -> void:
	combat_character = p_combat_character
	idle_state = CharacterStateMachineIdleState.new(self)
	current_state = idle_state

func new_tick() -> void:
	await current_state.new_tick()

func set_idle() -> void:
	current_state = idle_state

func _set_action(action: Action, from_body: BodyPart, to_body: BodyPart) -> void:
	var action_state := CharacterStateMachineActionState.new(self, action, from_body, to_body)
	current_state = action_state
	await action.prepare(from_body, to_body)
	character_renderer.expanded = false
	character_renderer.centered = false
