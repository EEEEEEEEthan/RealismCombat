@abstract
extends Item
class_name Weapon

# 基础挥砍伤害
func get_slash_damage() -> Damage:
	push_error("抽象基类")
	return Damage.new(0, 0, 0)

# 基础戳刺伤害
func get_pierce_damage() -> Damage:
	push_error("抽象基类")
	return Damage.new(0, 0, 0)
