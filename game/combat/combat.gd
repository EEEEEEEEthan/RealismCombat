extends Control
class_name Combat

const PLAYER_SIDE := 0
const ENEMY_SIDE := 1

var characters: Dictionary[Character, int] = {}
var character_renderers: Dictionary[Character, CharacterRenderer] = {}
var player_characters: Array[Character] = []
var enemy_characters: Array[Character] = []

func _ready() -> void:
	%CharacterLayer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	$DialogueArea.visible = false
	%PlayerFoldedReference.visible = false
	%PlayerReference.visible = false
	%EnemyReference.visible = false
	%EnemyFoldedReference.visible = false
	resized.connect(_refresh_character_targets)
	$SafeArea.resized.connect(_refresh_character_targets)
	_refresh_character_targets()

func _exit_tree() -> void:
	for character in characters.keys():
		if is_instance_valid(character.state_machine):
			character.on_exit_combat()

func add_character(character: Character, side: int) -> void:
	character.on_enter_combat()
	var character_renderer_scene := load(%CharacterPlaceHolder.get_instance_path()) as PackedScene
	var character_renderer := character_renderer_scene.instantiate() as CharacterRenderer
	character_renderer.bind(character)
	character_renderer.expanded = false
	character_renderer.centered_changed.connect(_refresh_character_targets)
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

func is_player_character(character: Character) -> bool:
	return characters[character] == PLAYER_SIDE

func get_character_renderer(character: Character) -> CharacterRenderer:
	return character_renderers[character]

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
		if character_renderer.centered:
			character_renderer.preferred_position = active_reference_position
			continue
		character_renderer.preferred_position = (
			folded_reference_rect.position
			+ Vector2(0.0, folded_reference_rect.size.y * folded_index)
		)
		folded_index += 1

func _get_reference_rect_in_character_layer(reference: Control) -> Rect2:
	var reference_global_rect := reference.get_global_rect()
	return Rect2(
		reference_global_rect.position - %CharacterLayer.global_position,
		reference_global_rect.size,
	)
