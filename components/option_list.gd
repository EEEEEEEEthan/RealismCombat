@tool
extends Node

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

func _init() -> void:
	_up = TextureRect.new()
	_up.texture = Resources.atlas_texture_theme_up
	_up.stretch_mode = TextureRect.STRETCH_KEEP_CENTERED
	_up.connect(&"mouse_entered", _on_hover_up)
	_up.focus_mode = Control.FOCUS_ALL
	add_child(_up, false, Node.INTERNAL_MODE_FRONT)
	_down = TextureRect.new()
	_down.stretch_mode = TextureRect.STRETCH_KEEP_CENTERED
	_down.texture = Resources.atlas_texture_theme_up
	_down.flip_v = true
	_down.connect(&"mouse_entered", _on_hover_down)
	_down.focus_mode = Control.FOCUS_ALL
	add_child(_down, false, Node.INTERNAL_MODE_BACK)
	connect(&"child_entered_tree", _on_child_entered_tree)
	connect(&"child_exiting_tree", _on_child_exiting_tree)

func _ready() -> void:
	_update_viewport()
	
func _on_child_entered_tree(node: Node) -> void:
	if node.get_parent() != self: return
	if node is not Control: return
	node.connect(&"focus_entered", _on_focus_changed)

func _on_child_exiting_tree(node: Node) -> void:
	node.disconnect(&"focus_entered", _on_focus_changed)

func _on_hover_up() -> void:
	if _show_up:
		viewport_begin -= 1

func _on_hover_down() -> void:
	if _show_down:
		viewport_begin += 1

func _on_focus_changed() -> void:
	if _focus_index <= viewport_begin and _show_up:
		viewport_begin -= 1
	elif _focus_index >= viewport_begin + viewport_count - 1 and _show_down:
		viewport_begin += 1
	_update_viewport()

func _update_viewport() -> void:
	var child_count = get_child_count()
	var viewport_end = viewport_begin + viewport_count
	for i in child_count:
		var child = get_child(i)
		child.visible = i >= viewport_begin && i < viewport_end
	_up.self_modulate = Color(1, 1, 1, 1 if _show_up else 0)
	_down.self_modulate = Color(1, 1, 1, 1 if _show_down else 0)
