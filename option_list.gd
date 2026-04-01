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
		return has_theme_color_override(&"font_color")
	set(value):
		if value:
			add_theme_color_override(&"font_color", font_color)
		else:
			remove_theme_color_override(&"font_color")
		notify_property_list_changed()
		_try_update(_update_all_button_appearance)

@export var font_color: Color = Color(1, 1, 1, 1):
	get:
		if not has_theme_color(&"font_color"):
			return get_theme_color(&"font_color", &"Button")
		return get_theme_color(&"font_color") if has_theme_color_override(&"font_color") else get_theme_color(&"font_color", &"OptionList")
	set(value):
		if override_font_color:
			add_theme_color_override(&"font_color", value)
		_try_update(_update_all_button_appearance)

@export var override_focus_color: bool = false:
	get:
		return has_theme_color_override(&"font_focus_color")
	set(value):
		if value:
			add_theme_color_override(&"font_focus_color", font_focus_color)
		else:
			remove_theme_color_override(&"font_focus_color")
		notify_property_list_changed()
		_try_update(_update_all_button_appearance)

@export var font_focus_color: Color = Color(0, 0, 0, 0):
	get:
		if not has_theme_color(&"font_focus_color"):
			return get_theme_color(&"font_focus_color", &"Button")
		return get_theme_color(&"font_focus_color") if has_theme_color_override(&"font_focus_color") else get_theme_color(&"font_focus_color", &"OptionList")
	set(value):
		if override_focus_color:
			add_theme_color_override(&"font_focus_color", value)
		_try_update(_update_all_button_appearance)

@export var override_disabled_color: bool = false:
	get:
		return has_theme_color_override(&"font_disabled_color")
	set(value):
		if value:
			add_theme_color_override(&"font_disabled_color", font_disabled_color)
		else:
			remove_theme_color_override(&"font_disabled_color")
		notify_property_list_changed()
		_try_update(_update_all_button_appearance)

@export var font_disabled_color: Color = Color(0, 0, 0, 0):
	get:
		if not has_theme_color(&"font_disabled_color"):
			return get_theme_color(&"font_disabled_color", &"Button")
		return get_theme_color(&"font_disabled_color") if has_theme_color_override(&"font_disabled_color") else get_theme_color(&"font_disabled_color", &"OptionList")
	set(value):
		if override_disabled_color:
			add_theme_color_override(&"font_disabled_color", value)
		_try_update(_update_all_button_appearance)

@export_subgroup("fonts")

@export var override_font: bool = false:
	get:
		return has_theme_font_override(&"font")
	set(value):
		if value:
			var resolved_font := font
			if resolved_font:
				add_theme_font_override(&"font", resolved_font)
			else:
				remove_theme_font_override(&"font")
		else:
			remove_theme_font_override(&"font")
		notify_property_list_changed()
		_try_update(_update_all_button_appearance)

@export var font: Font = null:
	get:
		if not has_theme_font(&"font"):
			return get_theme_font(&"font", &"Button")
		return get_theme_font(&"font") if has_theme_font_override(&"font") else get_theme_font(&"font", &"OptionList")
	set(value):
		if override_font:
			if value:
				add_theme_font_override(&"font", value)
			else:
				remove_theme_font_override(&"font")
		_try_update(_update_all_button_appearance)

@export var override_font_size: bool = false:
	get:
		return has_theme_font_size_override(&"font_size")
	set(value):
		if value:
			add_theme_font_size_override(&"font_size", font_size)
		else:
			remove_theme_font_size_override(&"font_size")
		notify_property_list_changed()
		_try_update(_update_all_button_appearance)

@export_range(0, 128) var font_size: int = 12:
	get:
		if not has_theme_font_size(&"font_size"):
			return get_theme_font_size(&"font_size", &"Button")
		return get_theme_font_size(&"font_size") if has_theme_font_size_override(&"font_size") else get_theme_font_size(&"font_size", &"OptionList")
	set(value):
		if override_font_size:
			add_theme_font_size_override(&"font_size", value)
		_try_update(_update_all_button_appearance)

@export_subgroup("icons")

@export var _override_indexer_icon: bool = false:
	get:
		return has_theme_icon_override(&"indexer_icon")
	set(value):
		if value:
			add_theme_icon_override(&"indexer_icon", _indexer_icon)
		else:
			remove_theme_icon_override(&"indexer_icon")
		notify_property_list_changed()
		_try_update(_update_theme)

@export var _indexer_icon: Texture2D:
	get:
		if not has_theme_icon(&"indexer_icon"):
			return get_theme_icon(&"arrow_collapsed", &"Tree")
		return get_theme_icon(&"indexer_icon") if has_theme_icon_override(&"indexer_icon") else get_theme_icon(&"indexer_icon", &"OptionList")
	set(value):
		if _override_indexer_icon:
			add_theme_icon_override(&"indexer_icon", value if value else get_theme_icon(&"arrow_collapsed", &"Tree"))
		_try_update(_update_theme)

signal option_focused(option_index: int)
signal option_selected(option_index: int)

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
	_options_container = VBoxContainer.new()
	_options_container.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_options_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_options_container.size_flags_vertical = Control.SIZE_EXPAND_FILL
	add_child(_options_container)
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
	var indexer_icon := _indexer_icon
	if indexer_icon:
		var focus_indicator_size := indexer_icon.get_size()
		var focus_indicator_spacing_width = focus_indicator_size.x
		_options_container.offset_left = focus_indicator_spacing_width
		_focus_indicator.texture = indexer_icon
		_focus_indicator.size = focus_indicator_size
	else:
		_options_container.offset_left = 0.0
		_focus_indicator.texture = null
		_focus_indicator.size = Vector2.ZERO
	_update_all_button_appearance()

func _update_viewport() -> void:
	var child_count = _options_container.get_child_count()
	for index in range(child_count - viewport_count):
		_options_container.get_child(child_count - index - 1).queue_free()
	for index in range(viewport_count - child_count):
		_options_container.add_child(_create_button())
	var option_count = _options.size()
	var visible_count = min(viewport_count, option_count)
	for index in range(visible_count):
		var button = _options_container.get_child(index) as Button
		if index == 0 and _viewport_start_index > 0:
			button.text = "...+" + str(_viewport_start_index + 1)
		elif viewport_count - 1 == index and _viewport_start_index + viewport_count < option_count:
			button.text = "...+" + str(option_count - (_viewport_start_index + viewport_count) + 1)
		elif index + _viewport_start_index < option_count:
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
	var option_count = _options.size()
	if button_index == 0 and _viewport_start_index > 0:
		_viewport_start_index -= 1
		button.get_parent().get_child(1).grab_focus()
		call_deferred(&"_update_viewport")
	elif button_index == viewport_count - 1 and _viewport_start_index + viewport_count < option_count:
		_viewport_start_index += 1
		button.get_parent().get_child(button_index - 1).grab_focus()
		call_deferred(&"_update_viewport")
	else:
		var option_index = button_index + _viewport_start_index
		if option_index >= 0 and option_index < option_count:
			option_focused.emit(option_index)

func _on_button_mouse_entered(button: Button) -> void:
	button.grab_focus()
	var button_index = button.get_index()
	var option_count = _options.size()
	var can_scroll_up = button_index == 0 and _viewport_start_index > 0
	var can_scroll_down = button_index == viewport_count - 1 and _viewport_start_index + viewport_count < option_count
	if can_scroll_up or can_scroll_down:
		_hovered_button = button
		_is_scrolling_up = can_scroll_up
		_hover_timer.start()
		return
	_hovered_button = null
	_hover_timer.stop()

func _on_button_mouse_exited(button: Button) -> void:
	if _hovered_button == button:
		_hovered_button = null
		_hover_timer.stop()

func _on_hover_timer_timeout() -> void:
	if not _hovered_button or not is_instance_valid(_hovered_button):
		_hovered_button = null
		_hover_timer.stop()
		return
	var button_index = _hovered_button.get_index()
	var option_count = _options.size()
	if _is_scrolling_up:
		if button_index == 0 and _viewport_start_index > 0:
			_viewport_start_index -= 1
			call_deferred(&"_update_viewport")
			call_deferred(&"_defer_refresh_hover_after_scroll", true)
			return
		_defer_refresh_hover_after_scroll(true)
		return
	if button_index == viewport_count - 1 and _viewport_start_index + viewport_count < option_count:
		_viewport_start_index += 1
		call_deferred(&"_update_viewport")
		call_deferred(&"_defer_refresh_hover_after_scroll", false)
		return
	_defer_refresh_hover_after_scroll(false)

func _defer_refresh_hover_after_scroll(scroll_up: bool) -> void:
	var child_count = _options_container.get_child_count()
	if child_count == 0:
		return
	var edge_index = 0 if scroll_up else child_count - 1
	var edge_button = _options_container.get_child(edge_index) as Button
	if edge_button.text.begins_with("..."):
		_hovered_button = edge_button
		return
	_hovered_button = null
	_hover_timer.stop()
	var start_index = 0 if scroll_up else child_count - 1
	var end_index = child_count if scroll_up else -1
	var index_step = 1 if scroll_up else -1
	for button_index in range(start_index, end_index, index_step):
		var button = _options_container.get_child(button_index) as Button
		if button.text != "" and not button.text.begins_with("..."):
			button.grab_focus()
			return

func _on_button_pressed(button: Button) -> void:
	var button_index = button.get_index()
	var option_count = _options.size()
	var option_index = button_index + _viewport_start_index
	if not button.disabled and option_index >= 0 and option_index < option_count and not button.text.begins_with("..."):
		option_selected.emit(option_index)

func _create_button() -> Button:
	var button = Button.new()
	button.focus_entered.connect(_on_button_focused.bind(button))
	button.focus_exited.connect(_refresh_focus_indicator)
	button.mouse_entered.connect(_on_button_mouse_entered.bind(button))
	button.mouse_exited.connect(_on_button_mouse_exited.bind(button))
	button.pressed.connect(_on_button_pressed.bind(button))
	button.item_rect_changed.connect(_refresh_focus_indicator)
	button["theme_override_styles/focus"] = _empty_style
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	return button

func _refresh_focus_indicator() -> void:
	if not is_node_ready() or not _indexer_icon:
		_focus_indicator.visible = false
		return
	var focused_button = get_viewport().gui_get_focus_owner() as Button
	if not focused_button or focused_button.get_parent() != _options_container:
		_focus_indicator.visible = false
		return
	var indicator_layer_global_position = _focus_indicator_layer.get_global_position()
	var focused_button_global_position = focused_button.get_global_position()
	var list_global_position = get_global_position()
	var focus_icon_x = list_global_position.x - indicator_layer_global_position.x
	_focus_indicator.position = Vector2(
		focus_icon_x,
		focused_button_global_position.y - indicator_layer_global_position.y + (focused_button.size.y - _focus_indicator.size.y) * 0.5
	)
	_focus_indicator.visible = true

func _update_all_button_appearance() -> void:
	var child_count = _options_container.get_child_count()
	var resolved_font := font
	var resolved_font_size := font_size
	var resolved_font_color := font_color
	var resolved_focus_color := font_focus_color
	var resolved_disabled_color := font_disabled_color
	for index in range(child_count):
		var button = _options_container.get_child(index) as Button
		button.icon = null
		button.add_theme_font_override(&"font", resolved_font)
		button.add_theme_font_size_override(&"font_size", resolved_font_size)
		button.add_theme_color_override(&"font_color", resolved_font_color)
		button.add_theme_color_override(&"font_focus_color", resolved_focus_color)
		button.add_theme_color_override(&"font_disabled_color", resolved_disabled_color)
	_refresh_focus_indicator()
