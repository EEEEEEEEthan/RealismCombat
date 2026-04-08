@tool
extends Range
class_name RatroProgressBar

func _notification(what: int) -> void:
	if what == NOTIFICATION_LAYOUT_DIRECTION_CHANGED:
		if is_layout_rtl():
			%Background.scale = Vector2(-1, 1)
		else:
			%Background.scale = Vector2(1, 1)

func _get_minimum_size() -> Vector2:
	return Vector2(3, 4)
