extends Resource
class_name OptionData

@export var text: String
@export var disabled: bool

func _init(option_text: String = "", option_disabled: bool = false) -> void:
	text = option_text
	disabled = option_disabled
