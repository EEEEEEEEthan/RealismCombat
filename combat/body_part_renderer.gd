extends HBoxContainer

@export var part: Defs.BodyPart = Defs.BodyPart.HEAD:
	set(v):
		part = v
		if is_node_ready():
			_refresh_part_label()

func set_hp(hp: int, max_hp: int) -> void:
	%ProgressBar.custom_minimum_size.x = max_hp * 2 + 1
	%ProgressBar.max_value = max_hp
	%ProgressBar.value = hp

func update_hp(hp: int) -> void:
	%ProgressBar.theme_type_variation = &"ProgressBarRed"
	%Label.self_modulate = Defs.COLOR_DARK_PINK
	# %ProgressBar.value 渐变到 hp
	%ProgressBar.theme_type_variation = &""
	%Label.self_modulate = Color.WHITE

func _ready() -> void:
	_refresh_part_label()
	%ProgressBar.step = 1

func _refresh_part_label() -> void:
	match part:
		Defs.BodyPart.HEAD:
			%Label.text = "头部"
		Defs.BodyPart.CHEST:
			%Label.text = "胸部"
		Defs.BodyPart.RIGHT_HAND:
			%Label.text = "右手"
		Defs.BodyPart.LEFT_HAND:
			%Label.text = "左手"
		Defs.BodyPart.RIGHT_FOOT:
			%Label.text = "右脚"
		Defs.BodyPart.LEFT_FOOT:
			%Label.text = "左脚"
		_:
			%Label.text = ""
