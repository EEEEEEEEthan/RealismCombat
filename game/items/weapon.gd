@abstract
extends Item
class_name Weapon

var swing_damage: Damage:
	get:
		if not swing_damage:
			var rate = quality.rate
			swing_damage = Damage.new(
				round(_base_swing_damage.slash * rate),
				round(_base_swing_damage.pierce * rate),
				round(_base_swing_damage.blunt * rate),
			)
		return swing_damage

var stab_damage: Damage:
	get:
		if not stab_damage:
			var rate = quality.rate
			stab_damage = Damage.new(
				round(_base_stab_damage.slash * rate),
				round(_base_stab_damage.pierce * rate),
				round(_base_stab_damage.blunt * rate),
			)
		return stab_damage

var _base_swing_damage: Damage:
	get:
		if not _base_swing_damage:
			push_error("待初始化")
			_base_swing_damage = Damage.new(1, 1, 1)
		return _base_swing_damage

var _base_stab_damage: Damage:
	get:
		if not _base_stab_damage:
			push_error("待初始化")
			_base_stab_damage = Damage.new(1, 1, 1)
		return _base_stab_damage

func get_name() -> StringName:
	return &"武器"

func get_title() -> StringName:
	if quality.value == 0:
		return &"裂开的"
	elif quality.value == 1:
		return &"弯曲的"
	elif quality.value == 2:
		return &""
	else:
		return &"平衡的"

func get_description() -> String:
	return "基础伤害:\n  挥砍:%s\n  戳刺:%s" % [swing_damage, stab_damage]

func _init() -> void:
	super._init()
