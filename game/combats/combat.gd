extends Control
class_name Combat

const PLAYER_SIDE := 0
const ENEMY_SIDE := 1

var characters: Dictionary[CombatCharacter, int] = {}
var character_renderers: Dictionary[CombatCharacter, CharacterRenderer] = {}
var player_characters: Array[CombatCharacter] = []
var enemy_characters: Array[CombatCharacter] = []
var _raw_to_combat: Dictionary[Character, CombatCharacter] = {}

func _ready() -> void:
	%CharacterLayer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	$DialogueArea.visible = false
	%PlayerFoldedReference.visible = false
	%PlayerReference.visible = false
	%EnemyReference.visible = false
	%EnemyFoldedReference.visible = false
	AudioManager.play_battle_bgm()

func _exit_tree() -> void:
	for combat_char: CombatCharacter in characters.keys():
		combat_char.state_machine = null
	_raw_to_combat.clear()
	AudioManager.play_menu_bgm()

func add_character(character: Character, side: int) -> void:
	var combat_char := CombatCharacter.new(self, character)
	_raw_to_combat[character] = combat_char
	var character_renderer_scene := load(%CharacterPlaceHolder.get_instance_path()) as PackedScene
	var character_renderer := character_renderer_scene.instantiate() as CharacterRenderer
	character_renderer.bind(combat_char)
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
	characters[combat_char] = side
	character_renderers[combat_char] = character_renderer
	if side == PLAYER_SIDE:
		player_characters.append(combat_char)
	else:
		enemy_characters.append(combat_char)

func run() -> void:
	while true:
		for combat_char: CombatCharacter in characters.keys():
			if not combat_char.alive:
				continue
			await combat_char.state_machine.new_tick()
		%Timer.start(0.1)
		await %Timer.timeout

func is_player_character(combat_character: CombatCharacter) -> bool:
	return characters[combat_character] == PLAYER_SIDE

func get_combat_character(raw: Character) -> CombatCharacter:
	return _raw_to_combat[raw]

## 战斗播报里角色名着色（最浅色档）；配色须与 CharacterRenderer 阵营一致。
func bbcode_character_name(combat_character: CombatCharacter) -> String:
	var family := (
		Defs.ColorFamily.OCEAN_BLUE
		if is_player_character(combat_character)
		else Defs.ColorFamily.FORGE_EMBER
	)
	var light_color := Defs.get_family_color(family, Defs.ColorShade.LIGHT)
	return "[color=#%s]%s[/color]" % [light_color.to_html(false), combat_character.character_name]

func get_character_renderer(combat_character: CombatCharacter) -> CharacterRenderer:
	return character_renderers[combat_character]

func _get_reference_rect_in_character_layer(reference: Control) -> Rect2:
	var reference_global_rect := reference.get_global_rect()
	return Rect2(
		reference_global_rect.position - %CharacterLayer.global_position,
		reference_global_rect.size,
	)
