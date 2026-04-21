extends Glove
class_name LeatherGlove


func _init() -> void:
	super._init()
	_base_protection = Protection.new(2, 2, 2)


func get_name() -> StringName:
	return &"皮手套"


func get_title() -> StringName:
	return &""


func get_description() -> String:
	return "鞣制皮手套，分左右手各穿一只。"
