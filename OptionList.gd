@tool
extends MarginContainer
class_name OptionList

static var _default_indicator_icon: Texture2D = ThemeDB.get_default_theme().get_icon("arrow_collapsed", "Tree")
static var _transparent_icon: ImageTexture = null
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
	set(value):
		override_font_color = value
		if override_font_color and font_color == Color(0, 0, 0, 0):
			var t := ThemeDB.get_default_theme()
			# 如果颜色还没设过（默认透明），用默认 Button 主题颜色初始化，避免开启 override 后变成不可见
			font_color = t.get_color("font_color", "Button")
		notify_property_list_changed()
		_try_update(_update_all_button_themes)

@export var override_focus_color: bool = false:
	set(value):
		override_focus_color = value
		if override_focus_color and font_focus_color == Color(0, 0, 0, 0):
			var t := ThemeDB.get_default_theme()
			font_focus_color = t.get_color("font_focus_color", "Button")
		notify_property_list_changed()
		_try_update(_update_all_button_themes)

@export var override_disabled_color: bool = false:
	set(value):
		override_disabled_color = value
		if override_disabled_color and font_disabled_color == Color(0, 0, 0, 0):
			var t := ThemeDB.get_default_theme()
			font_disabled_color = t.get_color("font_disabled_color", "Button")
		notify_property_list_changed()
		_try_update(_update_all_button_themes)

@export var font_color: Color = Color(0, 0, 0, 0):
	set(value):
		font_color = value
		_try_update(_update_all_button_themes)

@export var font_focus_color: Color = Color(0, 0, 0, 0):
	set(value):
		font_focus_color = value
		_try_update(_update_all_button_themes)

@export var font_disabled_color: Color = Color(0, 0, 0, 0):
	set(value):
		font_disabled_color = value
		_try_update(_update_all_button_themes)

@export_subgroup("fonts")

@export var override_font: bool = false:
	set(value):
		override_font = value
		if override_font and font == null:
			font = ThemeDB.get_default_theme().get_font("font", "Button")
		notify_property_list_changed()
		_try_update(_update_all_button_themes)

@export var override_font_size: bool = false:
	set(value):
		override_font_size = value
		if override_font_size and font_size <= 0:
			font_size = ThemeDB.get_default_theme().get_font_size("font_size", "Button")
		notify_property_list_changed()
		_try_update(_update_all_button_themes)

@export var font: Font = null:
	set(value):
		font = value
		_try_update(_update_all_button_themes)

@export_range(0, 128) var font_size: int = 0:
	set(value):
		font_size = value
		_try_update(_update_all_button_themes)

@export_subgroup("icons")

var _override_indexer_icon: bool = false:
	get:
		return has_theme_icon_override("indexer_icon")
	set(value):
		if value:
			add_theme_icon_override("indexer_icon", _indexer_icon if _indexer_icon else _default_indicator_icon)
		else:
			remove_theme_icon_override("indexer_icon")
		notify_property_list_changed()

var _indexer_icon: Texture2D = null:
	get:
		var icon = get("theme_override_icons/indexer_icon")
		if icon:
			return icon
		return _default_indicator_icon
	set(value):
		if _override_indexer_icon:
			add_theme_icon_override("indexer_icon", value if value else _default_indicator_icon)
		notify_property_list_changed()

signal option_focused(option_index: int)
signal option_selected(option_index: int)

var _options_container: VBoxContainer

var _viewport_start_index: int

var _hover_timer: Timer
var _hovered_button: Button = null
var _is_scrolling_up: bool = false  # true表示向上滚动，false表示向下滚动

func _validate_property(property: Dictionary) -> void:
	# 在编辑器中显示但不序列化
	if property.name == "_override_indexer_icon":
		property.usage = PROPERTY_USAGE_EDITOR
	# 只有当 _override_indexer_icon 为 true 时才显示 _indexer_icon，且不序列化
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
	add_child(_options_container)
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
		var icon_size = max(_indexer_icon.get_width(), _indexer_icon.get_height())
		if _transparent_icon == null or _transparent_icon.get_width() != icon_size:
			var image = Image.create(icon_size, icon_size, false, Image.FORMAT_RGBA8)
			image.fill(Color.TRANSPARENT)
			_transparent_icon = ImageTexture.create_from_image(image)
	_update_all_button_icons()
	_update_all_button_themes()

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
	_update_all_button_icons()
	_update_all_button_themes()

func _on_button_focused(button: Button) -> void:
	button.icon = _indexer_icon
	var button_index = button.get_index()
	if button_index == 0 and _viewport_start_index > 0:
		_viewport_start_index -= 1
		button.get_parent().get_child(1).grab_focus()
		call_deferred("_update_viewport")
	elif button_index == viewport_count - 1 and _viewport_start_index + viewport_count < len(_options):
		_viewport_start_index += 1
		button.get_parent().get_child(button_index - 1).grab_focus()
		call_deferred("_update_viewport")
	else:
		var option_index = _calculate_option_index(button_index)
		if option_index >= 0 and option_index < len(_options):
			option_focused.emit(option_index)

func _on_button_focus_lost(button: Button) -> void:
	button.icon = _transparent_icon

func _on_button_mouse_entered(button: Button) -> void:
	button.grab_focus()
	var button_index = button.get_index()
	# 如果是第一个按钮且还有更多选项（向上...+x）
	if button_index == 0 and _viewport_start_index > 0:
		_hovered_button = button
		_is_scrolling_up = true
		_hover_timer.start()
	# 如果是最后一个按钮且还有更多选项（向下...+4）
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
			# 向上滚动
			if button_index == 0 and _viewport_start_index > 0:
				_viewport_start_index -= 1
				call_deferred("_update_viewport")
				# 更新后重新设置hovered_button和焦点
				call_deferred("_update_hovered_button_after_scroll_up")
			else:
				# 滚动到顶了，让第一个实际选项获得焦点
				_hovered_button = null
				_hover_timer.stop()
				if _options_container.get_child_count() > 0:
					var first_button = _options_container.get_child(0) as Button
					if first_button and not first_button.text.begins_with("..."):
						first_button.grab_focus()
		else:
			# 向下滚动
			if button_index == viewport_count - 1 and _viewport_start_index + viewport_count < len(_options):
				_viewport_start_index += 1
				call_deferred("_update_viewport")
				# 更新后重新设置hovered_button和焦点
				call_deferred("_update_hovered_button_after_scroll_down")
			else:
				# 滚动到底了，让最后一个实际选项获得焦点
				_hovered_button = null
				_hover_timer.stop()
				if _options_container.get_child_count() > 0:
					var last_button = _options_container.get_child(viewport_count - 1) as Button
					if last_button and not last_button.text.begins_with("..."):
						last_button.grab_focus()
					else:
						# 如果最后一个还是...+x，找最后一个实际选项
						for i in range(viewport_count - 1, -1, -1):
							var btn = _options_container.get_child(i) as Button
							if btn and not btn.text.begins_with("...") and btn.text != "":
								btn.grab_focus()
								break
	else:
		_hovered_button = null
		_hover_timer.stop()

func _update_hovered_button_after_scroll_up() -> void:
	if _options_container.get_child_count() > 0:
		var first_button = _options_container.get_child(0) as Button
		if first_button and first_button.text.begins_with("..."):
			_hovered_button = first_button
		else:
			# 滚动到顶了，让第一个实际选项获得焦点
			_hovered_button = null
			_hover_timer.stop()
			if first_button and not first_button.text.begins_with("...") and first_button.text != "":
				first_button.grab_focus()
			else:
				# 如果第一个还是...+x，找第一个实际选项
				for i in range(viewport_count):
					var btn = _options_container.get_child(i) as Button
					if btn and not btn.text.begins_with("...") and btn.text != "":
						btn.grab_focus()
						break

func _update_hovered_button_after_scroll_down() -> void:
	if _options_container.get_child_count() > 0:
		var last_button = _options_container.get_child(viewport_count - 1) as Button
		if last_button and last_button.text.begins_with("..."):
			_hovered_button = last_button
		else:
			# 滚动到底了，让最后一个实际选项获得焦点
			_hovered_button = null
			_hover_timer.stop()
			if last_button and not last_button.text.begins_with("...") and last_button.text != "":
				last_button.grab_focus()
			else:
				# 如果最后一个还是...+x，找最后一个实际选项
				for i in range(viewport_count - 1, -1, -1):
					var btn = _options_container.get_child(i) as Button
					if btn and not btn.text.begins_with("...") and btn.text != "":
						btn.grab_focus()
						break

func _on_button_pressed(button: Button) -> void:
	var button_index = button.get_index()
	var option_index = _calculate_option_index(button_index)
	if not button.disabled and option_index >= 0 and option_index < len(_options) and not button.text.begins_with("..."):
		option_selected.emit(option_index)

func _create_button() -> Button:
	var button = Button.new()
	button.focus_entered.connect(_on_button_focused.bind(button))
	button.focus_exited.connect(_on_button_focus_lost.bind(button))
	button.mouse_entered.connect(_on_button_mouse_entered.bind(button))
	button.mouse_exited.connect(_on_button_mouse_exited.bind(button))
	button.pressed.connect(_on_button_pressed.bind(button))
	button["theme_override_styles/focus"] = _empty_style
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	if _transparent_icon:
		button.icon = _transparent_icon
	return button

func _calculate_option_index(button_index: int) -> int:
	return button_index + _viewport_start_index

func _update_all_button_icons() -> void:
	var node_count = _options_container.get_child_count()
	for i in range(node_count):
		var button = _options_container.get_child(i) as Button
		if button:
			if button.has_focus():
				button.icon = _indexer_icon
			else:
				button.icon = _transparent_icon

func _update_all_button_themes() -> void:
	var node_count = _options_container.get_child_count()
	for i in range(node_count):
		var button := _options_container.get_child(i) as Button
		if not button:
			continue

		# fonts
		if override_font and font:
			button.add_theme_font_override("font", font)
		else:
			button.remove_theme_font_override("font")

		if override_font_size and font_size > 0:
			button.add_theme_font_size_override("font_size", font_size)
		else:
			button.remove_theme_font_size_override("font_size")

		# colors（通过 toggle 控制是否覆盖；alpha==0 允许作为有效值，比如全透明字体）
		if override_font_color:
			button.add_theme_color_override("font_color", font_color)
		else:
			button.remove_theme_color_override("font_color")

		if override_focus_color:
			button.add_theme_color_override("font_focus_color", font_focus_color)
		else:
			button.remove_theme_color_override("font_focus_color")

		if override_disabled_color:
			button.add_theme_color_override("font_disabled_color", font_disabled_color)
		else:
			button.remove_theme_color_override("font_disabled_color")
