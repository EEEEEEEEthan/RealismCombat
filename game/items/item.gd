@abstract
class_name Item

var quality: PropertyInt = PropertyInt.new(4, 4)

var protection: Protection:
	get:
		if not protection:
			var rate = quality.rate
			protection = Protection.new(
				round(_base_protection.slash * rate),
				round(_base_protection.pierce * rate),
				round(_base_protection.blunt * rate),
			)
		return protection

var _base_protection: Protection:  # 每受到1伤害会有10%的概率品质下降
	get:
		if not _base_protection:
			push_error("待初始化")
			_base_protection = Protection.new(1, 1, 1)
		return _base_protection

func get_name() -> StringName:
	return &"道具"

func get_title() -> StringName:
	return &"一种"

func get_description() -> String:
	return &"一种道具"

func _to_string() -> String:
	return "%s%s" % [get_title(), get_name()]

func _init() -> void:
	quality.changed.connect(_on_quality_changed)


## 解除对 quality 的订阅。从背包等移除引用前须调用，否则 changed 会持有本实例导致无法释放。
func release_signal_bindings() -> void:
	if quality.changed.is_connected(_on_quality_changed):
		quality.changed.disconnect(_on_quality_changed)


func _on_quality_changed() -> void:
	protection = null


func get_item_slots() -> Array[ItemSlot]:
	return []
