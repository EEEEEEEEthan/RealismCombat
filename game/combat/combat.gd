extends Control
class_name Combat

const PLAYER_SIDE := 0
const ENEMY_SIDE := 1

var characters: Dictionary[Character, int] = {}
var character_renderers: Dictionary[Character, CharacterRenderer] = {}
var player_characters: Array[Character] = []
var enemy_characters: Array[Character] = []
var _turn_focus_character: Character
var _action_actor: Character
var _action_target: Character

func _ready() -> void:
	%CharacterLayer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_set_reference_visibility(false)
	resized.connect(_refresh_character_targets)
	$SafeArea.resized.connect(_refresh_character_targets)
	_refresh_character_targets()

func _exit_tree() -> void:
	for character in characters.keys():
		if is_instance_valid(character.state_machine):
			character.on_exit_combat()

func add_character(character: Character, side: int) -> void:
	character.on_enter_combat()
	var character_renderer := _new_character_renderer()
	character_renderer.bind(character)
	character_renderer.expanded = false
	if side == PLAYER_SIDE:
		character_renderer.layout_direction = Control.LAYOUT_DIRECTION_LTR
	else:
		character_renderer.layout_direction = Control.LAYOUT_DIRECTION_RTL
	%CharacterLayer.add_child(character_renderer)
	characters[character] = side
	character_renderers[character] = character_renderer
	if side == PLAYER_SIDE:
		player_characters.append(character)
	else:
		enemy_characters.append(character)
	_refresh_character_targets()
	character_renderer.position = character_renderer.preferred_position

func run() -> void:
	while true:
		for character: Character in characters.keys():
			if not character.alive:
				continue
			await character.state_machine.new_tick()
		%Timer.start(0.1)
		await %Timer.timeout

func _new_character_renderer() -> CharacterRenderer:
	var character_renderer_scene := load(%CharacterPlaceHolder.get_instance_path()) as PackedScene
	return character_renderer_scene.instantiate() as CharacterRenderer

func is_player_character(character: Character) -> bool:
	return characters[character] == PLAYER_SIDE

func present_turn_choice(character: Character) -> void:
	_turn_focus_character = character
	_action_actor = null
	_action_target = null
	_refresh_character_targets()
	_set_character_expanded(character, true)

func clear_turn_choice(character: Character) -> void:
	if _turn_focus_character != character:
		return
	_set_character_expanded(character, false)
	_turn_focus_character = null
	_refresh_character_targets()

func present_action_execution(attacker: Character, target: Character) -> void:
	_turn_focus_character = null
	_action_actor = attacker
	_action_target = target
	_refresh_character_targets()
	await _wait_for_characters([attacker, target])
	_set_character_expanded(attacker, true)
	_set_character_expanded(target, true)

func clear_action_execution() -> void:
	if _action_actor != null:
		_set_character_expanded(_action_actor, false)
	if _action_target != null and _action_target != _action_actor:
		_set_character_expanded(_action_target, false)
	_action_actor = null
	_action_target = null
	_refresh_character_targets()

func _refresh_character_targets() -> void:
	_refresh_side_targets(
		player_characters,
		%PlayerFoldedReference,
		%PlayerReference,
	)
	_refresh_side_targets(
		enemy_characters,
		%EnemyFoldedReference,
		%EnemyReference,
	)

func _refresh_side_targets(
	side_characters: Array[Character],
	folded_reference: Control,
	active_reference: Control,
) -> void:
	var folded_reference_rect := _get_reference_rect_in_character_layer(folded_reference)
	var active_reference_position := _get_reference_rect_in_character_layer(active_reference).position
	var folded_index := 0
	for character in side_characters:
		if not character_renderers.has(character):
			continue
		var character_renderer: CharacterRenderer = character_renderers[character]
		if character == _action_actor:
			character_renderer.z_index = 200
			character_renderer.preferred_position = active_reference_position
			continue
		if character == _action_target or character == _turn_focus_character:
			character_renderer.z_index = 150
			character_renderer.preferred_position = active_reference_position
			continue
		character_renderer.z_index = folded_index
		character_renderer.preferred_position = (
			folded_reference_rect.position
			+ Vector2(0.0, folded_reference_rect.size.y * folded_index)
		)
		folded_index += 1

func _wait_for_characters(characters_to_wait: Array[Character]) -> void:
	var waited_characters: Array[Character] = []
	for character in characters_to_wait:
		if character == null or waited_characters.has(character):
			continue
		waited_characters.append(character)
		if not character_renderers.has(character):
			continue
		var character_renderer: CharacterRenderer = character_renderers[character]
		await character_renderer.wait_until_preferred_position()

func _get_reference_rect_in_character_layer(reference: Control) -> Rect2:
	var reference_global_rect := reference.get_global_rect()
	return Rect2(
		reference_global_rect.position - %CharacterLayer.global_position,
		reference_global_rect.size,
	)

func _set_character_expanded(character: Character, should_expand: bool) -> void:
	if character == null or not character_renderers.has(character):
		return
	var character_renderer: CharacterRenderer = character_renderers[character]
	if character_renderer.expanded != should_expand:
		character_renderer.expanded = should_expand

func _set_reference_visibility(should_show: bool) -> void:
	$DialogueArea.visible = should_show
	%PlayerFoldedReference.visible = should_show
	%PlayerReference.visible = should_show
	%EnemyReference.visible = should_show
	%EnemyFoldedReference.visible = should_show
