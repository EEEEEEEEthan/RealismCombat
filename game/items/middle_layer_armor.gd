@abstract
extends Item
class_name MiddleLayerArmor

## 中层护甲（如锁甲）：其上可叠外层护甲、外套或腰带。
var over_slot: ItemSlot


func _init() -> void:
	super._init()
	over_slot = ItemSlot.new([OuterLayerArmor, OuterwearCoat, Belt])


func get_item_slots() -> Array[ItemSlot]:
	return [over_slot]
