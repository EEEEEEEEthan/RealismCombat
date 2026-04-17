extends Weapon
class_name ShortSword

func _init() -> void:
	super._init()
	_base_slash_damage = Damage.new(2, 0, 1)
	_base_pierce_damage = Damage.new(1, 1, 1)
	_base_protection = Protection.new(3, 99, 3)
