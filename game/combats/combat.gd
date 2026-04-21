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
	AudioManager.play_battle_bgm()

func _exit_tree() -> void:
	for character in characters.keys():
		if is_instance_valid(character.state_machine):
			character.on_exit_combat()
	AudioManager.play_menu_bgm()

func add_character(character: Character, side: int) -> void:
	character.on_enter_combat()
	var character_renderer_scene := load(%CharacterPlaceHolder.get_instance_path()) as PackedScene
	var character_renderer := character_renderer_scene.instantiate() as CharacterRenderer
	character_renderer.bind(character)
	character_renderer.expanded = false
	if side == PLAYER_SIDE:
		character_renderer.layout_direction = Control.LAYOUT_DIRECTION_LTR
	else:
		character_renderer.layout_direction = Control.LAYOUT_DIRECTION_RTL
	var folded_reference: Control
	var active_reference: Control
	var slot_index: int
	if side == PLAYER_SIDE:
		slot_index = player_characters.size()
		folded_reference = %PlayerFoldedReference
		active_reference = %PlayerReference
	else:
		slot_index = enemy_characters.size()
		folded_reference = %EnemyFoldedReference
		active_reference = %EnemyReference
	var folded_reference_rect := _get_reference_rect_in_character_layer(folded_reference)
	var active_reference_position := _get_reference_rect_in_character_layer(active_reference).position
	var original_position := (
		folded_reference_rect.position
		+ Vector2(0.0, folded_reference_rect.size.y * slot_index)
	)
	character_renderer.original_position = original_position
	character_renderer.active_position = active_reference_position
	%CharacterLayer.add_child(character_renderer)
	characters[character] = side
	character_renderers[character] = character_renderer
	if side == PLAYER_SIDE:
		player_characters.append(character)
	else:
		enemy_characters.append(character)

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

## 战斗播报里角色名着色（最浅色档）；配色须与 CharacterRenderer 阵营一致。
func bbcode_character_name(chr: Character) -> String:
	var family := (
		Defs.ColorFamily.OCEAN_BLUE
		if is_player_character(chr)
		else Defs.ColorFamily.FORGE_EMBER
	)
	var light_color := Defs.get_family_color(family, Defs.ColorShade.LIGHT)
	return "[color=#%s]%s[/color]" % [light_color.to_html(false), chr.character_name]

func get_character_renderer(character: Character) -> CharacterRenderer:
	return character_renderers[character]

func _get_reference_rect_in_character_layer(reference: Control) -> Rect2:
	var reference_global_rect := reference.get_global_rect()
	return Rect2(
		reference_global_rect.position - %CharacterLayer.global_position,
		reference_global_rect.size,
	)
