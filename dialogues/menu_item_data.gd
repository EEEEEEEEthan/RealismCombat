extends Resource
class_name MenuItemData

@export var text: String
@export var disabled: bool
@export var description: String
@export var pressed: Callable = Callable()

func _init(
	item_text: String = "",
	is_disabled: bool = false,
	item_description: String = "",
	item_pressed: Callable = Callable(),
) -> void:
	text = item_text
	disabled = is_disabled
	description = item_description
	pressed = item_pressed
