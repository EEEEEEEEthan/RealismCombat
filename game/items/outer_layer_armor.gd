@abstract
extends Item
class_name OuterLayerArmor

## 外层护甲（如板甲）：其上可穿外套或腰带。
var over_slot: ItemSlot


func _init() -> void:
	super._init()
	over_slot = ItemSlot.new([OuterwearCoat, Belt])


func get_item_slots() -> Array[ItemSlot]:
	return [over_slot]
