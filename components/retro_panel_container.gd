@tool
extends PanelContainer
class_name RetroPanelContainer

@export var fill_color_family: Defs.ColorFamily:
	set(v):
		fill_color_family = v
		_update_color()

@export var fill_color_shade: Defs.ColorShade:
	set(v):
		fill_color_shade = v
		_update_color()

var fill_color: Color:
	get: return Defs.get_family_color(fill_color_family, fill_color_shade)

var _fill: Control
var _color_rect_1: ColorRect
var _color_rect_2: ColorRect

func _init() -> void:
	_fill = Control.new()
	add_child(_fill)
	_color_rect_1 = ColorRect.new()
	_fill.add_child(_color_rect_1)
	_color_rect_1.set_anchor_and_offset(SIDE_TOP, 0, 0)
	_color_rect_1.set_anchor_and_offset(SIDE_BOTTOM, 1, 0)
	_color_rect_2 = ColorRect.new()
	_fill.add_child(_color_rect_2)
	_color_rect_2.set_anchor_and_offset(SIDE_TOP, 1, 0)
	_color_rect_2.set_anchor_and_offset(SIDE_RIGHT, 1, 0)
	_color_rect_2.set_anchor_and_offset(SIDE_BOTTOM, 1, 1)
	_color_rect_2.set_anchor_and_offset(SIDE_LEFT, 0, 0)
	_update_color()

func _ready() -> void:
	_update_layout_direction()

func _notification(what: int) -> void:
	if what == NOTIFICATION_LAYOUT_DIRECTION_CHANGED:
		_update_layout_direction()

func _update_color() -> void:
	_color_rect_1.self_modulate = fill_color
	_color_rect_2.self_modulate = fill_color

func _update_layout_direction() -> void:
	if is_layout_rtl():
		_color_rect_1.set_anchor_and_offset(SIDE_RIGHT, 1, 0)
		_color_rect_1.set_anchor_and_offset(SIDE_LEFT, 0, -1)
	else:
		_color_rect_1.set_anchor_and_offset(SIDE_RIGHT, 1, 1)
		_color_rect_1.set_anchor_and_offset(SIDE_LEFT, 0, 0)
