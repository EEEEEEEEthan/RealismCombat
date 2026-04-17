class_name Property

signal changed

var value: float:
	set(v):
		value = v
		changed.emit()

var max_value: float:
	set(v):
		max_value = v
		changed.emit()

var rate: float:
	get: return value / max_value

func _init(v: float, max_v: float) -> void:
	value = v
	max_value = max_v
