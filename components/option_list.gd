@tool
extends Node

@export var viewport_count: int:
	set(v):
		viewport_count = v
		_update_viewport()

@export var viewport_begin: int:
	set(v):
		viewport_begin = v
		_update_viewport()

func _ready() -> void:
	var up = TextureRect.new()
	var down = TextureRect.new()
	up.texture = Resources.atlas_texture_theme_up
	up.stretch_mode = TextureRect.STRETCH_KEEP_CENTERED
	down.stretch_mode = TextureRect.STRETCH_KEEP_CENTERED
	down.texture = Resources.atlas_texture_theme_up
	down.flip_v = true
	add_child(up, false, Node.INTERNAL_MODE_FRONT)
	add_child(down, false, Node.INTERNAL_MODE_BACK)
	up.connect(&"mouse_entered", _on_hover_up)
	down.connect(&"mouse_entered", _on_hover_down)
	_update_viewport()

func _on_hover_up() -> void:
	pass

func _on_hover_down() -> void:
	pass

func _update_viewport() -> void:
	var child_count = get_child_count()
	var viewport_end = viewport_begin + viewport_count
	for i in child_count:
		var child = get_child(i)
		child.visible = i >= viewport_begin && i < viewport_end
