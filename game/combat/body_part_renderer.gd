@tool
extends HBoxContainer
class_name BodyPartRenderer

var _part: BodyPart
var _label: Label
var _progress_bar: RetroProgressBar

@export var color_family: Defs.ColorFamily:
	set(v):
		color_family = v
		_update_color()

func _init() -> void:
	add_theme_constant_override("separation", 2)
	_label = Label.new()
	_label.name = &"Label"
	_label.text = &"头部"
	_label.unique_name_in_owner = true
	_label.theme_type_variation = &"LabelSmall"
	add_child(_label)
	_progress_bar = RetroProgressBar.new()
	_progress_bar.name = &"ProgressBar"
	_progress_bar.unique_name_in_owner = true
	_progress_bar.custom_minimum_size = Vector2(19, 4)
	_progress_bar.size_flags_horizontal = Control.SIZE_FILL
	_progress_bar.size_flags_vertical = Control.SIZE_SHRINK_CENTER
	add_child(_progress_bar)

func _ready() -> void:
	_update_color()

func setup(part: BodyPart) -> void:
	_part = part
	_label.text = part.part_name
	_progress_bar.custom_minimum_size.x = part.hp.max_value * 2 - 1
	_progress_bar.max_value = part.hp.max_value
	_progress_bar.value = part.hp.value
	part.hp.changed.connect(_on_hp_changed)

func _on_hp_changed() -> void:
	_progress_bar.red = true
	_progress_bar.value = _part.hp.value
	_label.self_modulate = Defs.get_family_color(Defs.ColorFamily.ROSE_PINK, Defs.ColorShade.DARK)
	await get_tree().create_timer(0.2).timeout
	_label.self_modulate = Color.WHITE
	_progress_bar.red = false

func _update_color() -> void:
	if not is_node_ready(): return
	_progress_bar.color_family = color_family
