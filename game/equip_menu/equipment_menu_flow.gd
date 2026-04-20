extends RefCounted
class_name EquipmentMenuFlow

## 主菜单中「装备」项的流程：角色列表 → 身体部位 → 槽位（显式物品名或空）→ 槽内有物品则递归子槽并支持卸下；空槽则从背包选匹配物品装上。
## 依赖 Dialogues.create_menu_dialogue() 与 MenuItemData，与 Game.player_side_characters、Game.inventory 交互；装上/卸下时须同步背包与 ItemSlot.item，嵌套时注意引用归属与循环引用防护。

var _game: Game
var _nav := EquipmentNavigationStack.new()


func enter(game: Game) -> void:
	_game = game
	_nav.clear()
	# TODO: 实现分层菜单循环：每层末尾「返回」对应 pop 或退出；嵌套物品槽复用「展示槽列表」同一套构建函数（与需求第 3、4 步一致）
	push_error("EquipmentMenuFlow.enter：未实现")
	return


func _show_player_characters_menu() -> void:
	# TODO: 选项为 player_side_characters 各角色名 + 返回；选择后 push 帧并进入身体部位菜单
	push_error("EquipmentMenuFlow._show_player_characters_menu：未实现")


func _show_body_parts_menu(_character: Character) -> void:
	# TODO: 选项为 character.all_body_parts 的 part_name() + 返回
	push_error("EquipmentMenuFlow._show_body_parts_menu：未实现")


func _show_slots_for_body_part(_character: Character, _body_part: BodyPart) -> void:
	# TODO: 用 body_part.get_item_slots() 生成选项，标签为槽内物品名或「空」+ 返回
	push_error("EquipmentMenuFlow._show_slots_for_body_part：未实现")


func _show_slots_for_item(_character: Character, _host_item: Item) -> void:
	# TODO: 同 _show_slots_for_body_part 的展示逻辑，数据源改为 host_item.get_item_slots()；额外提供「卸下」与「返回」
	push_error("EquipmentMenuFlow._show_slots_for_item：未实现")


func _show_pick_from_inventory_to_slot(_inventory: Inventory, _slot: ItemSlot) -> void:
	# TODO: 列表来自 slot.matching_inventory_items(inventory)；选后设置 slot.item 并从 inventory 移除（规则与背包单件引用一致时再定）
	push_error("EquipmentMenuFlow._show_pick_from_inventory_to_slot：未实现")


func _unequip_slot_to_inventory(_slot: ItemSlot, _inventory: Inventory) -> void:
	# TODO: slot.item 置空，物品回到 inventory；若嵌套在另一 Item 的槽内需明确父引用更新
	push_error("EquipmentMenuFlow._unequip_slot_to_inventory：未实现")
