extends Footwear
class_name LeatherBoot


func _init(p_side: Defs.Side) -> void:
	super(p_side)
	_base_protection = Protection.new(3, 2, 3)


func get_name() -> StringName:
	match side:
		Defs.Side.LEFT:
			return &"皮鞋(左)"
		_:
			return &"皮鞋(右)"


func get_title() -> StringName:
	return &""


func get_description() -> String:
	return "简易皮靴，分左右脚各穿一只。"
