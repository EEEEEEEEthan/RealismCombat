extends HBoxContainer

const HP_VALUE_TWEEN_DURATION_SEC := 0.2

@export var part: Defs.BodyPart = Defs.BodyPart.HEAD:
	set(v):
		part = v
		if is_node_ready():
			_refresh_part_label()

@onready var part_label: Label = %Label
@onready var hp_progress_bar: ProgressBar = %ProgressBar

func set_hp(hp: int, max_hp: int) -> void:
	hp_progress_bar.custom_minimum_size.x = max_hp * 2 + 1
	hp_progress_bar.max_value = max_hp
	hp_progress_bar.value = hp

func update_hp(hp: int) -> void:
	var hp_display_tween: Tween = create_tween()
	hp_progress_bar.theme_type_variation = &"ProgressBarRed"
	part_label.self_modulate = Defs.COLOR_DARK_PINK
	hp_display_tween.tween_property(hp_progress_bar, "value", float(hp), HP_VALUE_TWEEN_DURATION_SEC)
	hp_display_tween.tween_callback(func() -> void:
		hp_progress_bar.theme_type_variation = &""
		part_label.self_modulate = Color.WHITE
	)

func _ready() -> void:
	_refresh_part_label()
	hp_progress_bar.step = 1

func _refresh_part_label() -> void:
	match part:
		Defs.BodyPart.HEAD:
			part_label.text = "头部"
		Defs.BodyPart.CHEST:
			part_label.text = "胸部"
		Defs.BodyPart.RIGHT_HAND:
			part_label.text = "右手"
		Defs.BodyPart.LEFT_HAND:
			part_label.text = "左手"
		Defs.BodyPart.RIGHT_FOOT:
			part_label.text = "右脚"
		Defs.BodyPart.LEFT_FOOT:
			part_label.text = "左脚"
		_:
			part_label.text = ""
