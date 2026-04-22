extends Object
class_name SaveSlotPresentation

## [param is_load_mode] 为选读档为真：空槽/损坏为 disabled，否则为开新周目用选槽行。
static func build_table_rows(is_load_mode: bool) -> Array[MenuItemData]:
	var items: Array[MenuItemData] = []
	for slot_index in range(SaveSlots.SLOT_COUNT):
		var preview: SaveSlotPreview = SaveSnapshot.preview_path(SaveSlots.path_for_slot(slot_index))
		if preview.is_empty:
			if is_load_mode:
				items.append(
					MenuItemData.new(
						"#%d 空" % (slot_index + 1), true, "该槽没有存档",
					),
				)
			else:
				items.append(
					MenuItemData.new(
						"#%d 空" % (slot_index + 1), false, "在此槽开始新游戏",
					),
				)
		elif preview.snapshot == null:
			if is_load_mode:
				items.append(
					MenuItemData.new(
						"#%d 已损坏" % (slot_index + 1), true, "数据无法解析",
					),
				)
			else:
				items.append(
					MenuItemData.new(
						"#%d 已损坏" % (slot_index + 1), false, "可覆盖此槽并重新开始",
					),
				)
		else:
			var snap: SaveSnapshot = preview.snapshot
			var line_text: String = "#%d %s" % [slot_index + 1, snap.save_name]
			var ver: String = snap.game_version.to_display_string()
			var time_label: String = SaveSnapshot.format_time_ago(
				snap.saved_at_unix,
			)
			var desc: String
			if is_load_mode:
				desc = "版本 %s\n%s\n\n读取此进度" % [ver, time_label]
				items.append(MenuItemData.new(line_text, false, desc))
			else:
				desc = "版本 %s\n%s\n\n覆盖并开始新游戏" % [ver, time_label]
				items.append(MenuItemData.new(line_text, false, desc))
	items.append(MenuItemData.new("返回", false, "返回主菜单"))
	return items
