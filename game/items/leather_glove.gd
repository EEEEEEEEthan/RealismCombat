extends Glove
class_name LeatherGlove


func _init(p_side: Defs.Side) -> void:
	super(p_side)
	_base_protection = Protection.new(2, 2, 2)


func get_name() -> StringName:
	match side:
		Defs.Side.LEFT:
			return &"皮手套(左)"
		_:
			return &"皮手套(右)"


func get_title() -> StringName:
	return &""


func get_description() -> String:
	return "鞣制皮手套，分左右手各穿一只。"
