extends HBoxContainer

@onready var part_label: Label = %Label

@export var part: Enums.BodyPart = Enums.BodyPart.HEAD:
	set(value):
		part = value
		if is_node_ready():
			_refresh_part_label()

func _ready() -> void:
	_refresh_part_label()

func _refresh_part_label() -> void:
	match part:
		Enums.BodyPart.HEAD:
			part_label.text = "头部"
		Enums.BodyPart.CHEST:
			part_label.text = "胸部"
		Enums.BodyPart.RIGHT_HAND:
			part_label.text = "右手"
		Enums.BodyPart.LEFT_HAND:
			part_label.text = "左手"
		Enums.BodyPart.RIGHT_FOOT:
			part_label.text = "右脚"
		Enums.BodyPart.LEFT_FOOT:
			part_label.text = "左脚"
		_:
			part_label.text = ""
