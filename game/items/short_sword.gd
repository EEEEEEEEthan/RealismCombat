extends Weapon
class_name ShortSword

func get_name() -> StringName:
	return &"短剑"

func get_description() -> String:
	return "一种基础武器\n\n%s" % [super.get_description()]

func _init() -> void:
	super._init()
	_base_swing_damage = Damage.new(2, 0, 1)
	_base_stab_damage = Damage.new(1, 1, 1)
	_base_protection = Protection.new(3, 99, 3)
