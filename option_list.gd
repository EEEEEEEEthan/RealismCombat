@tool
extends MarginContainer
class_name OptionList

static var _empty_style: StyleBoxEmpty = StyleBoxEmpty.new()

@export_range(3, 64) var viewport_count: int = 8:
	set(value):
		viewport_count = value
		_try_update(_update_viewport)

@export var _options: PackedStringArray:
	set(value):
		if len(value) > 64:
			value = value.slice(0, 64)
		_options = value
		_try_update(_update_viewport)

@export var _disabled_mask: int:
	set(value):
		_disabled_mask = value
		_try_update(_update_viewport)

@export_group("Theme Overrides")

@export_subgroup("colors")

@export var override_font_color: bool = false:
	get:
		return has_theme_color_override("font_color")
	set(v):
		if v:
			add_theme_color_override("font_color", font_color)
		else:
			remove_theme_color_override("font_color")
		notify_property_list_changed()
		_try_update(_update_all_button_appearance)

@export var font_color: Color = Color(1, 1, 1, 1):
	get:
		if not has_theme_color_override("font_color"):
			return get_theme_color("font_color", "Button")
		if has_theme_color_override("font_color"):
			return get_theme_color("font_color")
		return get_theme_color("font_color", "OptionList")
	set(v):
		if override_font_color:
			add_theme_color_override("font_color", v)
		_try_update(_update_all_button_appearance)

@export var override_focus_color: bool = false:
	get:
		return has_theme_color_override("font_focus_color")
	set(v):
		if v:
			add_theme_color_override("font_focus_color", font_focus_color)
		else:
			remove_theme_color_override("font_focus_color")
		notify_property_list_changed()
		_try_update(_update_all_button_appearance)

@export var font_focus_color: Color = Color(0, 0, 0, 0):
	get:
		if not has_theme_color("font_focus_color"):
			return get_theme_color("font_focus_color", "Button")
		if has_theme_color_override("font_focus_color"):
			return get_theme_color("font_focus_color")
		return get_theme_color("font_focus_color", "OptionList")
	set(v):
		if override_focus_color:
			add_theme_color_override("font_focus_color", v)
		_try_update(_update_all_button_appearance)

@export var override_disabled_color: bool = false:
	get:
		return has_theme_color_override("font_disabled_color")
	set(v):
		if v:
			add_theme_color_override("font_disabled_color", font_disabled_color)
		else:
			remove_theme_color_override("font_disabled_color")
		notify_property_list_changed()
		_try_update(_update_all_button_appearance)

@export var font_disabled_color: Color = Color(0, 0, 0, 0):
	get:
		if not has_theme_color("font_disabled_color"):
			return get_theme_color("font_disabled_color", "Button")
		if has_theme_color_override("font_disabled_color"):
			return get_theme_color("font_disabled_color")
		return get_theme_color("font_disabled_color", "OptionList")
	set(v):
		if override_disabled_color:
			add_theme_color_override("font_disabled_color", v)
		_try_update(_update_all_button_appearance)

@export_subgroup("fonts")

@export var override_font: bool = false:
	get:
		return has_theme_font_override("font")
	set(v):
		if v:
			var resolved_font := font
			if resolved_font:
				add_theme_font_override("font", resolved_font)
			else:
				remove_theme_font_override("font")
		else:
			remove_theme_font_override("font")
		notify_property_list_changed()
		_try_update(_update_all_button_appearance)

@export var font: Font = null:
	get:
		if not has_theme_font("font"):
			return get_theme_font("font", "Button")
		if has_theme_font_override("font"):
			return get_theme_font("font")
		return get_theme_font("font", "OptionList")
	set(v):
		if override_font:
			if v:
				add_theme_font_override("font", v)
			else:
				remove_theme_font_override("font")
		_try_update(_update_all_button_appearance)

@export var override_font_size: bool = false:
	get:
		return has_theme_font_size_override("font_size")
	set(v):
		if v:
			add_theme_font_size_override("font_size", font_size)
		else:
			remove_theme_font_size_override("font_size")
		notify_property_list_changed()
		_try_update(_update_all_button_appearance)

@export_range(0, 128) var font_size: int = 12:
	get:
		if not has_theme_font_size("font_size"):
			return get_theme_font_size("font_size", "Button")
		if has_theme_font_size_override("font_size"):
			return get_theme_font_size("font_size")
		return get_theme_font_size("font_size", "OptionList")
	set(v):
		if override_font_size:
			add_theme_font_size_override("font_size", v)
		_try_update(_update_all_button_appearance)

@export_subgroup("icons")

@export var _override_indexer_icon: bool = false:
	get:
		return has_theme_icon_override("indexer_icon")
	set(v):
		if v:
			add_theme_icon_override("indexer_icon", _indexer_icon)
		else:
			remove_theme_icon_override("indexer_icon")
		notify_property_list_changed()
		_try_update(_update_theme)

@export var _indexer_icon: Texture2D:
	get:
		if not has_theme_icon("indexer_icon"):
			return get_theme_icon("arrow_collapsed", "Tree")
		if has_theme_icon_override("indexer_icon"):
			return get_theme_icon("indexer_icon")
		return get_theme_icon("indexer_icon", "OptionList")
	set(v):
		if _override_indexer_icon:
			add_theme_icon_override("indexer_icon", v if v else get_theme_icon("arrow_collapsed", "Tree"))
		_try_update(_update_theme)

signal option_focused(option_index: int)
signal option_selected(option_index: int)

var _layout_container: HBoxContainer
var _focus_indicator_spacer: Control
var _options_container: VBoxContainer
var _focus_indicator_layer: Control
var _focus_indicator: TextureRect

var _viewport_start_index: int

var _hover_timer: Timer
var _hovered_button: Button = null
var _is_scrolling_up: bool = false

func _validate_property(property: Dictionary) -> void:
	if property.name == "_override_indexer_icon":
		property.usage = PROPERTY_USAGE_EDITOR
	elif property.name == "_indexer_icon":
		if _override_indexer_icon:
			property.usage = PROPERTY_USAGE_EDITOR
			property.hint = PROPERTY_HINT_RESOURCE_TYPE
			property.hint_string = "Texture2D"
		else:
			property.usage = PROPERTY_USAGE_NO_EDITOR
	elif property.name == "font_color":
		if not override_font_color:
			property.usage = PROPERTY_USAGE_NO_EDITOR
	elif property.name == "font_focus_color":
		if not override_focus_color:
			property.usage = PROPERTY_USAGE_NO_EDITOR
	elif property.name == "font_disabled_color":
		if not override_disabled_color:
			property.usage = PROPERTY_USAGE_NO_EDITOR
	elif property.name == "font":
		if not override_font:
			property.usage = PROPERTY_USAGE_NO_EDITOR
	elif property.name == "font_size":
		if not override_font_size:
			property.usage = PROPERTY_USAGE_NO_EDITOR

func _notification(notification_type: int) -> void:
	if notification_type == NOTIFICATION_THEME_CHANGED and is_node_ready():
		_update_theme()

func _ready() -> void:
	_layout_container = HBoxContainer.new()
	_layout_container.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_layout_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_layout_container.size_flags_vertical = Control.SIZE_EXPAND_FILL
	add_child(_layout_container)
	_focus_indicator_spacer = Control.new()
	_focus_indicator_spacer.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_layout_container.add_child(_focus_indicator_spacer)
	_options_container = VBoxContainer.new()
	_options_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_options_container.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_layout_container.add_child(_options_container)
	_focus_indicator_layer = Control.new()
	_focus_indicator_layer.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_focus_indicator_layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_focus_indicator_layer)
	_focus_indicator = TextureRect.new()
	_focus_indicator.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_focus_indicator.stretch_mode = TextureRect.STRETCH_KEEP
	_focus_indicator.visible = false
	_focus_indicator_layer.add_child(_focus_indicator)
	_hover_timer = Timer.new()
	_hover_timer.wait_time = 0.2
	_hover_timer.one_shot = false
	_hover_timer.timeout.connect(_on_hover_timer_timeout)
	add_child(_hover_timer)
	_try_update(_update_theme)
	_try_update(_update_viewport)

func _try_update(update_function: Callable) -> void:
	if is_node_ready():
		update_function.call()

func _update_theme() -> void:
	if _indexer_icon:
		var focus_indicator_size := _indexer_icon.get_size()
		var focus_indicator_spacing_width = focus_indicator_size.x
		_focus_indicator_spacer.custom_minimum_size = Vector2(focus_indicator_spacing_width, 0)
		_focus_indicator.texture = _indexer_icon
		_focus_indicator.size = focus_indicator_size
	else:
		_focus_indicator_spacer.custom_minimum_size = Vector2.ZERO
		_focus_indicator.texture = null
		_focus_indicator.size = Vector2.ZERO
	_update_all_button_appearance()

func _update_viewport() -> void:
	var node_count = _options_container.get_child_count()
	for index in range(node_count - viewport_count):
		_options_container.get_child(node_count - index - 1).queue_free()
	for index in range(viewport_count - node_count):
		_options_container.add_child(_create_button())
	var visible_count = min(viewport_count, len(_options))
	for index in range(visible_count):
		var button = _options_container.get_child(index) as Button
		if index == 0 and _viewport_start_index > 0:
			button.text = "...+" + str(_viewport_start_index + 1)
		elif viewport_count - 1 == index and _viewport_start_index + viewport_count < len(_options):
			button.text = "...+" + str(len(_options) - (_viewport_start_index + viewport_count) + 1)
		elif index + _viewport_start_index < len(_options):
			button.text = _options[index + _viewport_start_index]
		else:
			button.text = ""
		button.disabled = (((1 << index) & _disabled_mask) != 0)
	for index in range(visible_count, viewport_count):
		(_options_container.get_child(index) as Button).text = ""
	_update_all_button_appearance()

func _on_button_focused(button: Button) -> void:
	_refresh_focus_indicator()
	var button_index = button.get_index()
	if button_index == 0 and _viewport_start_index > 0:
		_viewport_start_index -= 1
		button.get_parent().get_child(1).grab_focus()
		call_deferred(&"_update_viewport")
	elif button_index == viewport_count - 1 and _viewport_start_index + viewport_count < len(_options):
		_viewport_start_index += 1
		button.get_parent().get_child(button_index - 1).grab_focus()
		call_deferred(&"_update_viewport")
	else:
		var option_index = _calculate_option_index(button_index)
		if option_index >= 0 and option_index < len(_options):
			option_focused.emit(option_index)

func _on_button_focus_lost() -> void:
	_refresh_focus_indicator()

func _on_button_mouse_entered(button: Button) -> void:
	button.grab_focus()
	var button_index = button.get_index()
	if button_index == 0 and _viewport_start_index > 0:
		_hovered_button = button
		_is_scrolling_up = true
		_hover_timer.start()
	elif button_index == viewport_count - 1 and _viewport_start_index + viewport_count < len(_options):
		_hovered_button = button
		_is_scrolling_up = false
		_hover_timer.start()
	else:
		_hovered_button = null
		_hover_timer.stop()

func _on_button_mouse_exited(button: Button) -> void:
	if _hovered_button == button:
		_hovered_button = null
		_hover_timer.stop()

func _on_hover_timer_timeout() -> void:
	if _hovered_button and is_instance_valid(_hovered_button):
		var button_index = _hovered_button.get_index()
		if _is_scrolling_up:
			if button_index == 0 and _viewport_start_index > 0:
				_viewport_start_index -= 1
				call_deferred(&"_update_viewport")
				call_deferred(&"_defer_refresh_hover_after_scroll", true)
			else:
				_hovered_button = null
				_hover_timer.stop()
				if _options_container.get_child_count() > 0:
					var first_button = _options_container.get_child(0) as Button
					if first_button and not first_button.text.begins_with("..."):
						first_button.grab_focus()
		else:
			if button_index == viewport_count - 1 and _viewport_start_index + viewport_count < len(_options):
				_viewport_start_index += 1
				call_deferred(&"_update_viewport")
				call_deferred(&"_defer_refresh_hover_after_scroll", false)
			else:
				_hovered_button = null
				_hover_timer.stop()
				if _options_container.get_child_count() > 0:
					var last_button = _options_container.get_child(viewport_count - 1) as Button
					if last_button and not last_button.text.begins_with("..."):
						last_button.grab_focus()
					else:
						for scroll_index in range(viewport_count - 1, -1, -1):
							var scroll_button = _options_container.get_child(scroll_index) as Button
							if scroll_button and not scroll_button.text.begins_with("...") and scroll_button.text != "":
								scroll_button.grab_focus()
								break
	else:
		_hovered_button = null
		_hover_timer.stop()

func _defer_refresh_hover_after_scroll(scroll_up: bool) -> void:
	if _options_container.get_child_count() == 0:
		return
	if scroll_up:
		var first_button = _options_container.get_child(0) as Button
		if first_button and first_button.text.begins_with("..."):
			_hovered_button = first_button
		else:
			_hovered_button = null
			_hover_timer.stop()
			if first_button and not first_button.text.begins_with("...") and first_button.text != "":
				first_button.grab_focus()
			else:
				for scroll_index in range(viewport_count):
					var scroll_button = _options_container.get_child(scroll_index) as Button
					if scroll_button and not scroll_button.text.begins_with("...") and scroll_button.text != "":
						scroll_button.grab_focus()
						break
	else:
		var last_button = _options_container.get_child(viewport_count - 1) as Button
		if last_button and last_button.text.begins_with("..."):
			_hovered_button = last_button
		else:
			_hovered_button = null
			_hover_timer.stop()
			if last_button and not last_button.text.begins_with("...") and last_button.text != "":
				last_button.grab_focus()
			else:
				for scroll_index in range(viewport_count - 1, -1, -1):
					var scroll_button = _options_container.get_child(scroll_index) as Button
					if scroll_button and not scroll_button.text.begins_with("...") and scroll_button.text != "":
						scroll_button.grab_focus()
						break

func _on_button_pressed(button: Button) -> void:
	var button_index = button.get_index()
	var option_index = _calculate_option_index(button_index)
	if not button.disabled and option_index >= 0 and option_index < len(_options) and not button.text.begins_with("..."):
		option_selected.emit(option_index)

func _create_button() -> Button:
	var button = Button.new()
	button.focus_entered.connect(_on_button_focused.bind(button))
	button.focus_exited.connect(_on_button_focus_lost)
	button.mouse_entered.connect(_on_button_mouse_entered.bind(button))
	button.mouse_exited.connect(_on_button_mouse_exited.bind(button))
	button.pressed.connect(_on_button_pressed.bind(button))
	button.item_rect_changed.connect(_refresh_focus_indicator)
	button["theme_override_styles/focus"] = _empty_style
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	return button

func _calculate_option_index(button_index: int) -> int:
	return button_index + _viewport_start_index

func _find_focused_button() -> Button:
	var node_count = _options_container.get_child_count()
	for index in range(node_count):
		var button = _options_container.get_child(index) as Button
		if button and button.has_focus():
			return button
	return null

func _refresh_focus_indicator() -> void:
	if not is_node_ready() or not _indexer_icon:
		_focus_indicator.visible = false
		return
	var focused_button = _find_focused_button()
	if not focused_button:
		_focus_indicator.visible = false
		return
	var indicator_layer_global_position = _focus_indicator_layer.get_global_position()
	var focused_button_global_position = focused_button.get_global_position()
	_focus_indicator.position = Vector2(
		0,
		focused_button_global_position.y - indicator_layer_global_position.y + (focused_button.size.y - _focus_indicator.size.y) * 0.5
	)
	_focus_indicator.visible = true

func _update_all_button_appearance() -> void:
	var node_count = _options_container.get_child_count()
	for index in range(node_count):
		var button = _options_container.get_child(index) as Button
		if not button:
			continue
		button.icon = null
		button.add_theme_font_override("font", font)
		button.add_theme_font_size_override("font_size", font_size)
		button.add_theme_color_override("font_color", font_color)
		button.add_theme_color_override("font_focus_color", font_focus_color)
		button.add_theme_color_override("font_disabled_color", font_disabled_color)
	_refresh_focus_indicator()
