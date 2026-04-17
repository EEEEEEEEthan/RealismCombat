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

var rate: float:
	get: return float(value) / max_value

func _init(v: int, max_v: int) -> void:
	value = v
	max_value = max_v
