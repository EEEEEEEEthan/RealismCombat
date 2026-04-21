@abstract
extends Item
class_name InnerLiningGarment

## 内衬：其上可叠中层护甲、外套或腰带。
var over_slot: ItemSlot


func _init() -> void:
	super._init()
	over_slot = ItemSlot.new([MiddleLayerArmor, OuterwearCoat, Belt])


func get_item_slots() -> Array[ItemSlot]:
	return [over_slot]
