extends HBoxContainer

@export var part: Enums.BodyPart = Enums.BodyPart.HEAD:
	set(v):
		part = v
		if is_node_ready():
			_refresh_part_label()

func update_hp(current_hp: int, max_hp: int, immedate: bool = false) -> void:
	if immedate:
		%ProgressBar.custom_minimum_size.x = max_hp * 2 + 1
		%ProgressBar.max_value = max_hp
		%ProgressBar.value = current_hp

func _ready() -> void:
	_refresh_part_label()
	%ProgressBar.step = 1

func _refresh_part_label() -> void:
	match part:
		Enums.BodyPart.HEAD:
			%Label.text = "头部"
		Enums.BodyPart.CHEST:
			%Label.text = "胸部"
		Enums.BodyPart.RIGHT_HAND:
			%Label.text = "右手"
		Enums.BodyPart.LEFT_HAND:
			%Label.text = "左手"
		Enums.BodyPart.RIGHT_FOOT:
			%Label.text = "右脚"
		Enums.BodyPart.LEFT_FOOT:
			%Label.text = "左脚"
		_:
			%Label.text = ""
