extends PanelContainer
class_name GenericDialogue


signal pressed(index: int)


var _tw_gen: int = 0
var _last_selected_index: int = -1

var active: bool = true:
	set(value):
		if active == value:
			return
		active = value
		if is_node_ready():
			_refresh_active()

var text: String = "":
	set(value):
		text = value
		if not is_node_ready():
			return
		var old_total := rich_text_label.get_total_character_count()
		var old_vis := rich_text_label.visible_characters
		rich_text_label.text = value
		_apply_preserved_visible(old_total, old_vis)
		_update_chrome_visibility()

var options: Array = []:
	set(value):
		options = value
		if not is_node_ready():
			return
		_rebuild_option_buttons()
		_update_chrome_visibility()

@onready var continue_button: TextureButton = %Continue
@onready var options_container: HBoxContainer = %Options
@onready var rich_text_label: RichTextLabel = %RichTextLabel
@onready var typewriter_timer: Timer = %TypewriterTimer

func _ready() -> void:
	continue_button.pressed.connect(func(): pressed.emit(0))
	continue_button.focus_entered.connect(func(): _last_selected_index = -1)
	_rebuild_option_buttons()
	var old_total := rich_text_label.get_total_character_count()
	var old_vis := rich_text_label.visible_characters
	rich_text_label.text = text
	_apply_preserved_visible(old_total, old_vis)
	_update_chrome_visibility()
	_refresh_active()
	_typewriter_loop.call_deferred()

func clear() -> void:
	_tw_gen += 1
	text = ""
	options = []

func _notification(what: int) -> void:
	if what != NOTIFICATION_VISIBILITY_CHANGED:
		return
	if not is_node_ready():
		return
	if visible and active:
		_refresh_active()

func _apply_preserved_visible(old_total: int, old_visible: int) -> void:
	var new_total := rich_text_label.get_total_character_count()
	if new_total <= 0:
		rich_text_label.visible_characters = -1
		return
	var shown: int
	if old_total <= 0:
		shown = 0
	elif old_visible < 0 or old_visible >= old_total:
		shown = old_total
	else:
		shown = old_visible
	rich_text_label.visible_characters = mini(shown, new_total)

func _update_chrome_visibility() -> void:
	var revealed := _fully_revealed()
	continue_button.visible = revealed and options.is_empty()
	for option_index in range(options.size()):
		_get_option_button(option_index).visible = revealed
	_refresh_button_states()
	if revealed and active and visible:
		_restore_focus.call_deferred()

func _fully_revealed() -> bool:
	var total := rich_text_label.get_total_character_count()
	if total <= 0:
		return true
	var vis := rich_text_label.visible_characters
	return vis < 0 or vis >= total

func _typewriter_loop() -> void:
	await get_tree().process_frame
	while is_inside_tree():
		var gen := _tw_gen
		var total := rich_text_label.get_total_character_count()

		if total <= 0:
			rich_text_label.visible_characters = -1
			if gen == _tw_gen:
				_update_chrome_visibility()
			while gen == _tw_gen and rich_text_label.get_total_character_count() <= 0:
				await get_tree().process_frame
			continue

		var vis: int = rich_text_label.visible_characters
		if vis < 0:
			vis = total
		vis = clampi(vis, 0, total)

		if vis < total:
			while vis < total and gen == _tw_gen:
				vis += 1
				rich_text_label.visible_characters = vis
				typewriter_timer.start(0.02)
				await typewriter_timer.timeout
			if gen != _tw_gen:
				continue
			_update_chrome_visibility()

		while gen == _tw_gen:
			var t2 := rich_text_label.get_total_character_count()
			if t2 <= 0:
				break
			var v2: int = rich_text_label.visible_characters
			if v2 < 0:
				v2 = t2
			if v2 < t2:
				break
			await get_tree().process_frame

func _refresh_active() -> void:
	_refresh_button_states()
	if active and visible:
		_restore_focus.call_deferred()

func _rebuild_option_buttons() -> void:
	for child_node in options_container.get_children():
		if child_node == continue_button:
			continue
		child_node.free()
	for option_index in range(options.size()):
		var idx := option_index
		var option_button := RetroButton.new()
		var option: MenuItemData = options[option_index]
		option_button.text = option.text
		option_button.disabled = option.disabled
		option_button.focus_entered.connect(func(): _last_selected_index = idx)
		option_button.pressed.connect(func(): pressed.emit(idx))
		options_container.add_child(option_button)

func _refresh_button_states() -> void:
	var continue_can_focus := active and continue_button.visible
	continue_button.mouse_filter = (
		Control.MOUSE_FILTER_STOP
		if continue_can_focus
		else Control.MOUSE_FILTER_IGNORE
	)
	continue_button.focus_mode = Control.FOCUS_ALL if continue_can_focus else Control.FOCUS_NONE
	for option_index in range(options.size()):
		var option_button := _get_option_button(option_index)
		var option: MenuItemData = options[option_index]
		var can_focus: bool = active and option_button.visible and not option.text.is_empty()
		option_button.mouse_filter = Control.MOUSE_FILTER_STOP if can_focus else Control.MOUSE_FILTER_IGNORE
		option_button.focus_mode = Control.FOCUS_ALL if can_focus else Control.FOCUS_NONE

func _restore_focus() -> void:
	if not active or not visible:
		return
	if options.is_empty():
		if continue_button.visible:
			continue_button.grab_focus()
		return
	var option_index := -1
	if _can_focus_option(_last_selected_index):
		option_index = _last_selected_index
	else:
		for i in range(options.size()):
			if _can_focus_option(i):
				option_index = i
				break
	if option_index < 0:
		return
	_last_selected_index = option_index
	_get_option_button(option_index).grab_focus()

func _can_focus_option(option_index: int) -> bool:
	if option_index < 0 or option_index >= options.size():
		return false
	if options[option_index].text.is_empty():
		return false
	return _get_option_button(option_index).visible

func _get_option_button(option_index: int) -> RetroButton:
	return options_container.get_child(option_index + 1) as RetroButton
