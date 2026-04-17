@abstract
class_name Item

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

var quality: PropertyInt = PropertyInt.new(4, 4)

func _init() -> void:
	quality.changed.connect(_on_quality_changed)

func _on_quality_changed() -> void:
	protection = null
