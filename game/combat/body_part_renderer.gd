@tool
extends HBoxContainer
class_name BodyPartRenderer

var _part: BodyPart

func setup(part: BodyPart) -> void:
	_part = part
	%Label.text = part.part_name
	%ProgressBar.custom_minimum_size.x = part.hp.max_value * 2 - 1
	%ProgressBar.max_value = part.hp.max_value
	%ProgressBar.value = part.hp.value
	part.hp.changed.connect(_on_hp_changed)

func _on_hp_changed() -> void:
	var hp_display_tween: Tween = create_tween()
	%ProgressBar.theme_type_variation = &"ProgressBarRed"
	%Label.self_modulate = Defs.COLOR_DARK_PINK
	hp_display_tween.tween_property(%ProgressBar, "value", _part.hp.value, 0.2)
	hp_display_tween.tween_callback(func() -> void:
		%ProgressBar.theme_type_variation = &""
		%Label.self_modulate = Color.WHITE
	)
