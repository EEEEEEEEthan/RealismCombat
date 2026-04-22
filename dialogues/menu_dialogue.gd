extends PanelContainer
class_name MenuDialogue


signal pressed(index: int)


var title: String = "":
	set(value):
		title = value
		name = "MenuDialogue:" + value
		_refresh_title()

var options: Array[MenuItemData] = []:
	set(value):
		options = value
		_refresh_options()

@onready var rich_text_label: RichTextLabel = %RichTextLabel
@onready var retro_scroll_container: RetroScrollContainer = %RetroScrollContainer
@onready var title_label: RichTextLabel = %Title

func _ready() -> void:
	retro_scroll_container.navigation_selection_changed.connect(func(_i): AudioManager.play_sound_effect(%Audios.audio_stream_hover))
	_refresh_title()
	_refresh_options()
	_refresh_interaction()

func _notification(what: int) -> void:
	if what != NOTIFICATION_VISIBILITY_CHANGED:
		return
	if not is_node_ready():
		return
	_refresh_interaction()

func _refresh_title() -> void:
	if not is_node_ready(): return
	title_label.text = title

func _refresh_options() -> void:
	if not is_node_ready(): return
	var child_count = retro_scroll_container.get_child_count()
	var length = options.size()
	if child_count < length:
		for _i in range(length - child_count):
			var button = RetroButton.new()
			retro_scroll_container.add_child(button)
			_connect_button(button)
	elif child_count > length:
		for removal_offset in range(child_count - length):
			retro_scroll_container.get_child(child_count - removal_offset - 1).queue_free()
	for option_index in range(length):
		var button: RetroButton = retro_scroll_container.get_child(option_index)
		var option: MenuItemData = options[option_index]
		button.alignment = HORIZONTAL_ALIGNMENT_LEFT
		button.text = option.text
		button.disabled = option.disabled
	_refresh_button_states()
	_refresh_description()

func _refresh_interaction() -> void:
	_refresh_button_states()
	if visible:
		_restore_focus.call_deferred()

func _connect_button(button: RetroButton) -> void:
	button.focus_entered.connect(_on_focus_button.bind(button))
	button.pressed.connect(_on_press_button.bind(button))

func _on_focus_button(button: RetroButton) -> void:
	var option_index := button.get_index()
	if option_index < 0 or option_index >= options.size():
		return
	rich_text_label.text = options[option_index].description

func _on_press_button(button: RetroButton) -> void:
	var option_index := button.get_index()
	if option_index < 0 or option_index >= options.size():
		return
	if options[option_index].disabled:
		AudioManager.play_sound_effect(%Audios.disabled_press)
		return
	AudioManager.play_sound_effect(%Audios.audio_stream_press)
	pressed.emit(option_index)

func _refresh_button_states() -> void:
	for option_index in range(options.size()):
		var button: RetroButton = retro_scroll_container.get_child(option_index)
		var option: MenuItemData = options[option_index]
		var can_focus := visible and not option.text.is_empty()
		button.mouse_filter = Control.MOUSE_FILTER_STOP if can_focus else Control.MOUSE_FILTER_IGNORE
		button.focus_mode = Control.FOCUS_ALL if can_focus else Control.FOCUS_NONE

func _refresh_description() -> void:
	var option_index := _get_preferred_option_index()
	if option_index < 0:
		rich_text_label.text = ""
		return
	rich_text_label.text = options[option_index].description

func _restore_focus() -> void:
	if not visible:
		return
	var option_index := _get_preferred_option_index()
	if option_index < 0:
		return
	retro_scroll_container.last_selected = option_index
	var button: Control = retro_scroll_container.get_child(option_index)
	button.grab_focus()

func _get_preferred_option_index() -> int:
	var option_index := retro_scroll_container.last_selected
	if _can_focus_option(option_index):
		return option_index
	for fallback_option_index in range(options.size()):
		if _can_focus_option(fallback_option_index):
			return fallback_option_index
	return -1

func _can_focus_option(option_index: int) -> bool:
	return (
		option_index >= 0
		and option_index < options.size()
		and not options[option_index].text.is_empty()
	)
