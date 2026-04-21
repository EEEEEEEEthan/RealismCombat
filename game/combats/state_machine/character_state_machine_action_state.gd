extends RefCounted
class_name CharacterStateMachineActionState

var machine: CharacterStateMachine
var _windup: float
var _action: Action
var _from_body: BodyPart
var _to_body: BodyPart

var action_name: StringName:
	get:
		if _action == null:
			return &""
		return _action.get_name()

func _init(
	p_machine: CharacterStateMachine,
	action: Action,
	from_body: BodyPart,
	to_body: BodyPart,
) -> void:
	machine = p_machine
	_action = action
	_from_body = from_body
	_to_body = to_body
	_windup = action.get_windup_action_points()

func new_tick() -> void:
	_windup -= machine.action_points_per_tick
	if _windup <= 0:
		var participating_renderers: Array[CharacterRenderer] = []
		for participant_character in [_from_body.character, _to_body.character]:
			var character_renderer := machine.combat.get_character_renderer(participant_character)
			if participating_renderers.has(character_renderer):
				continue
			participating_renderers.append(character_renderer)
			character_renderer.centered = true
		for character_renderer in participating_renderers:
			character_renderer.expanded = true
		(await _action.execute(_from_body, _to_body)).queue_free()
		for character_renderer in participating_renderers:
			character_renderer.expanded = false
			character_renderer.centered = false
		machine.set_idle()
		machine.action_points.value -= _action.get_recovery_action_points()
		_action = null
		_from_body = null
		_to_body = null
