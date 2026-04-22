extends Control
class_name Combat

const PLAYER_SIDE := 0
const ENEMY_SIDE := 1

var characters: Dictionary[Battler, int] = {}
var character_renderers: Dictionary[Battler, CharacterRenderer] = {}
var player_characters: Array[Battler] = []
var enemy_characters: Array[Battler] = []
var _raw_to_battler: Dictionary[Character, Battler] = {}

var audio_stream_hit: AudioStream:
	get: return %Audios.audio_stream_hit

var audio_stream_dodge: AudioStream:
	get: return %Audios.audio_stream_dodge

func _ready() -> void:
	%CharacterLayer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	$DialogueArea.visible = false
	%PlayerFoldedReference.visible = false
	%PlayerReference.visible = false
	%EnemyReference.visible = false
	%EnemyFoldedReference.visible = false
	AudioManager.play_background_music(%Audios.audio_stream_background_music)

func _exit_tree() -> void:
	for battler: Battler in characters.keys():
		battler.cleanup()
		battler.state_machine = null
	_raw_to_battler.clear()
	AudioManager.play_menu_bgm()

func add_character(character: Character, side: int) -> void:
	var battler := Battler.new(self, character)
	_raw_to_battler[character] = battler
	var character_renderer_scene := load(%CharacterPlaceHolder.get_instance_path()) as PackedScene
	var character_renderer := character_renderer_scene.instantiate() as CharacterRenderer
	character_renderer.bind(battler)
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
	characters[battler] = side
	character_renderers[battler] = character_renderer
	if side == PLAYER_SIDE:
		player_characters.append(battler)
	else:
		enemy_characters.append(battler)

func run() -> void:
	while true:
		if await _try_finish_battle():
			return
		for battler: Battler in characters.keys():
			if not battler.alive:
				continue
			await battler.state_machine.new_tick()
		%Timer.start(0.1)
		await %Timer.timeout


## 若任一阵营全灭则弹出结算对话并结束本战斗节点（由 Game 再开新实例）。
func _try_finish_battle() -> bool:
	if _side_has_alive_battler(player_characters):
		if _side_has_alive_battler(enemy_characters):
			return false
		await _show_battle_result_dialogue(true)
	else:
		await _show_battle_result_dialogue(false)
	queue_free()
	return true


func _side_has_alive_battler(side_list: Array[Battler]) -> bool:
	for b: Battler in side_list:
		if b.alive:
			return true
	return false


func _show_battle_result_dialogue(player_victory: bool) -> void:
	var dialogue := Dialogues.create_generic_dialogue()
	dialogue.text = "战斗胜利！" if player_victory else "战斗失败…"
	AudioManager.stop_bgm()
	AudioManager.play_sound_effect(%Audios.audio_stream_victory)
	await dialogue.pressed
	dialogue.queue_free()

func is_player_character(battler: Battler) -> bool:
	return characters[battler] == PLAYER_SIDE

func get_battler(raw: Character) -> Battler:
	return _raw_to_battler[raw]

## 战斗播报里角色名着色（最浅色档）；配色须与 CharacterRenderer 阵营一致。
func bbcode_character_name(battler: Battler) -> String:
	var family := (
		Defs.ColorFamily.OCEAN_BLUE
		if is_player_character(battler)
		else Defs.ColorFamily.FORGE_EMBER
	)
	var light_color := Defs.get_family_color(family, Defs.ColorShade.LIGHT)
	return "[color=#%s]%s[/color]" % [light_color.to_html(false), battler.character_name]

func get_character_renderer(battler: Battler) -> CharacterRenderer:
	return character_renderers[battler]

func _get_reference_rect_in_character_layer(reference: Control) -> Rect2:
	var reference_global_rect := reference.get_global_rect()
	return Rect2(
		reference_global_rect.position - %CharacterLayer.global_position,
		reference_global_rect.size,
	)
