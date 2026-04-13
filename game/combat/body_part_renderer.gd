@tool
extends HBoxContainer
class_name BodyPartRenderer

const _RETRO_PROGRESS_SCRIPT: Script = preload("res://components/retro_progress_bar.gd")

var _part: BodyPart
var _label: Label
var _progress_bar: Range

func _ready() -> void:
	_build_ui()

func _build_ui() -> void:
	if _label:
		return
	add_theme_constant_override("separation", 2)
	_label = Label.new()
	_label.name = &"Label"
	_label.unique_name_in_owner = true
	_label.theme_type_variation = &"LabelSmall"
	add_child(_label)
	_progress_bar = Range.new()
	_progress_bar.name = &"ProgressBar"
	_progress_bar.unique_name_in_owner = true
	_progress_bar.custom_minimum_size = Vector2(19, 4)
	# 与旧 tscn 一致：仅 SIZE_FILL(1)，勿用 SIZE_EXPAND_FILL，否则在 HBox 里被拉满，段数与 max 不符
	_progress_bar.size_flags_horizontal = Control.SIZE_FILL
	_progress_bar.size_flags_vertical = Control.SIZE_SHRINK_CENTER
	_progress_bar.set_script(_RETRO_PROGRESS_SCRIPT)
	add_child(_progress_bar)

func setup(part: BodyPart) -> void:
	_build_ui()
	_part = part
	_label.text = part.part_name
	_progress_bar.custom_minimum_size.x = part.hp.max_value * 2 - 1
	_progress_bar.max_value = part.hp.max_value
	_progress_bar.value = part.hp.value
	part.hp.changed.connect(_on_hp_changed)

func _on_hp_changed() -> void:
	var hp_display_tween: Tween = create_tween()
	_progress_bar.theme_type_variation = &"ProgressBarRed"
	_label.self_modulate = Defs.COLOR_DARK_PINK
	hp_display_tween.tween_property(_progress_bar, "value", _part.hp.value, 0.2)
	hp_display_tween.tween_callback(func() -> void:
		_progress_bar.theme_type_variation = &""
		_label.self_modulate = Color.WHITE
	)
