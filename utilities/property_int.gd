class_name PropertyInt

signal changed

var value: int:
	set(v):
		value = v
		changed.emit()

var max_value: int:
	set(v):
		max_value = v
		changed.emit()

func _init(v: int, max_v: int) -> void:
	value = v
	max_value = max_v
