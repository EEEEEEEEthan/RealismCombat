extends RefCounted
class_name SaveSlots

const SLOT_COUNT := 6

static func path_for_slot(slot_index: int) -> String:
	return "user://save_slot_%d.sav" % (slot_index + 1)

static func file_exists(slot_index: int) -> bool:
	return FileAccess.file_exists(path_for_slot(slot_index))


static func debug_print_slot_state(tag: String) -> void:
	print("[存档调试][%s] OS.get_user_data_dir() = %s" % [tag, OS.get_user_data_dir()])
	print("[存档调试][%s] globalize user:// = %s" % [tag, ProjectSettings.globalize_path("user://")])
	var app_name: Variant = ProjectSettings.get_setting("application/config/name", "")
	print("[存档调试][%s] application/config/name = %s" % [tag, app_name])
	for slot_index in range(SLOT_COUNT):
		var user_path := path_for_slot(slot_index)
		var abs_path := ProjectSettings.globalize_path(user_path)
		var exists := file_exists(slot_index)
		print("[存档调试][%s] 槽%d user=%s | abs=%s | 存在=%s" % [tag, slot_index + 1, user_path, abs_path, exists])
