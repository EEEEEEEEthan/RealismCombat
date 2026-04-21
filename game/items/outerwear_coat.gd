@abstract
extends Item
class_name OuterwearCoat

## 外套（罩袍等）：其上仅可系腰带。
var over_slot: ItemSlot


func _init() -> void:
	super._init()
	over_slot = ItemSlot.new([Belt])


func get_item_slots() -> Array[ItemSlot]:
	return [over_slot]
