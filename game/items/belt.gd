@abstract
extends Item
class_name Belt

## 腰带：含若干武器槽，槽数量由子类在构造时传入。
var weapon_slots: Array[ItemSlot] = []


func _init(weapon_slot_count: int) -> void:
	super._init()
	for _i in weapon_slot_count:
		weapon_slots.append(ItemSlot.new([Weapon]))


func get_item_slots() -> Array[ItemSlot]:
	return weapon_slots
