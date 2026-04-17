extends Weapon
class_name ShortSword

var __slash_damage: Damage = Damage.new(2, 0, 1)
var __pierce_damage: Damage = Damage.new(0, 2, 1)

# 基础挥砍伤害
func get_slash_damage() -> Damage:
	return __slash_damage

# 基础戳刺伤害
func get_pierce_damage() -> Damage:
	return __pierce_damage
