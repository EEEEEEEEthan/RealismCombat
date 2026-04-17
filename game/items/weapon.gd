@abstract
extends Item
class_name Weapon

var slash_damage: Damage:
	get:
		if not slash_damage:
			var rate = quality.rate
			slash_damage = Damage.new(
				round(_base_slash_damage.slash * rate),
				round(_base_slash_damage.pierce * rate),
				round(_base_slash_damage.blunt * rate),
			)
		return slash_damage

var pierce_damage: Damage:
	get:
		if not pierce_damage:
			var rate = quality.rate
			pierce_damage = Damage.new(
				round(_base_pierce_damage.slash * rate),
				round(_base_pierce_damage.pierce * rate),
				round(_base_pierce_damage.blunt * rate),
			)
		return pierce_damage

var _base_slash_damage: Damage:
	get:
		if not _base_slash_damage:
			push_error("待初始化")
			_base_slash_damage = Damage.new(1, 1, 1)
		return _base_slash_damage

var _base_pierce_damage: Damage:
	get:
		if not _base_pierce_damage:
			push_error("待初始化")
			_base_pierce_damage = Damage.new(1, 1, 1)
		return _base_pierce_damage

func _init() -> void:
	super._init()
