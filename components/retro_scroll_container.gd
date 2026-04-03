@tool
extends VBoxContainer
class_name RetroScrollContainer

@export var viewport_begin: int:
	set(v):
		viewport_begin = v
		_update_viewport()

@export var viewport_count: int:
	set(v):
		viewport_count = v
		_update_viewport()

var _show_up: bool:
	get:
		return viewport_begin > 0

var _show_down: bool:
	get:
		return viewport_begin + viewport_count < get_child_count()

var _focus_index: int:
	get:
		for i in get_child_count():
			var child: Control = get_child(i)
			if child.has_focus():
				return i
		return -1

var _up: TextureRect
var _down: TextureRect
var _hover_scroll_direction: int = 0
var _hover_scroll_accum_sec: float = 0.0

const _HOVER_SCROLL_INTERVAL_SEC := 0.5

func _init() -> void:
	connect(&"child_entered_tree", _on_child_entered_tree)
	connect(&"child_exiting_tree", _on_child_exiting_tree)

func _ready() -> void:
	_up = TextureRect.new()
	_up.texture = Resources.atlas_texture_theme_up
	_up.stretch_mode = TextureRect.STRETCH_KEEP_CENTERED
	_up.connect(&"mouse_entered", _on_hover_up_entered)
	_up.connect(&"mouse_exited", _on_hover_up_exited)
	_down = TextureRect.new()
	_down.stretch_mode = TextureRect.STRETCH_KEEP_CENTERED
	_down.texture = Resources.atlas_texture_theme_up
	_down.flip_v = true
	_down.connect(&"mouse_entered", _on_hover_down_entered)
	_down.connect(&"mouse_exited", _on_hover_down_exited)
	set_process(false)
	add_child(_up, false, Node.INTERNAL_MODE_FRONT)
	add_child(_down, false, Node.INTERNAL_MODE_BACK)
	_update_viewport()
	
func _on_child_entered_tree(node: Node) -> void:
	if node.get_parent() != self: return
	if node is not Control: return
	node.connect(&"focus_entered", _on_focus_changed)
	_update_viewport()

func _on_child_exiting_tree(node: Node) -> void:
	node.disconnect(&"focus_entered", _on_focus_changed)
	_update_viewport()

func _on_hover_up_entered() -> void:
	_hover_scroll_direction = -1
	_hover_scroll_accum_sec = 0.0
	_scroll_viewport_by_hover()
	if _hover_scroll_direction != 0:
		set_process(true)

func _on_hover_up_exited() -> void:
	if _hover_scroll_direction == -1:
		_hover_scroll_direction = 0
		_hover_scroll_accum_sec = 0.0
		_stop_hover_scroll_process_if_idle()

func _on_hover_down_entered() -> void:
	_hover_scroll_direction = 1
	_hover_scroll_accum_sec = 0.0
	_scroll_viewport_by_hover()
	if _hover_scroll_direction != 0:
		set_process(true)

func _on_hover_down_exited() -> void:
	if _hover_scroll_direction == 1:
		_hover_scroll_direction = 0
		_hover_scroll_accum_sec = 0.0
		_stop_hover_scroll_process_if_idle()

func _scroll_viewport_by_hover() -> void:
	if _hover_scroll_direction == -1 and _show_up:
		viewport_begin -= 1
	elif _hover_scroll_direction == 1 and _show_down:
		viewport_begin += 1
	else:
		_hover_scroll_direction = 0

func _process(delta: float) -> void:
	if _hover_scroll_direction == 0:
		return
	_hover_scroll_accum_sec += delta
	while _hover_scroll_accum_sec >= _HOVER_SCROLL_INTERVAL_SEC and _hover_scroll_direction != 0:
		_hover_scroll_accum_sec -= _HOVER_SCROLL_INTERVAL_SEC
		_scroll_viewport_by_hover()
	if _hover_scroll_direction == 0:
		_hover_scroll_accum_sec = 0.0
		_stop_hover_scroll_process_if_idle()

func _stop_hover_scroll_process_if_idle() -> void:
	if _hover_scroll_direction == 0:
		set_process(false)

func _on_focus_changed() -> void:
	if not _is_focus_change_from_ui_navigation():
		return
	while _focus_index >= 0 and _focus_index <= viewport_begin and _show_up:
		viewport_begin -= 1
	while _focus_index >= 0 and _focus_index >= viewport_begin + viewport_count - 1 and _show_down:
		viewport_begin += 1
	_update_viewport()

func _is_focus_change_from_ui_navigation() -> bool:
	return (
		Input.is_action_just_pressed(&"ui_up")
		or Input.is_action_just_pressed(&"ui_down")
		or Input.is_action_just_pressed(&"ui_left")
		or Input.is_action_just_pressed(&"ui_right")
		or Input.is_action_just_pressed(&"ui_focus_next")
		or Input.is_action_just_pressed(&"ui_focus_prev")
	)

func _update_viewport() -> void:
	_update_viewport_immediate()

func _update_viewport_immediate() -> void:
	if not is_node_ready():
		return
	var child_count = get_child_count()
	var viewport_end = viewport_begin + viewport_count
	for i in child_count:
		var child = get_child(i)
		child.visible = i >= viewport_begin && i < viewport_end
	_up.self_modulate = Color(1, 1, 1, 1 if _show_up else 0)
	_down.self_modulate = Color(1, 1, 1, 1 if _show_down else 0)
