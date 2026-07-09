@tool
extends EditorImportPlugin

var _rgb_line_pattern: RegEx


func _init() -> void:
	_rgb_line_pattern = RegEx.new()
	_rgb_line_pattern.compile("^\\s*(\\d{1,3})\\s+(\\d{1,3})\\s+(\\d{1,3})\\b")


func _get_importer_name() -> String:
	return "godot.gpl_color_palette"


func _get_visible_name() -> String:
	return "Godot GPL 色板"


func _get_recognized_extensions() -> PackedStringArray:
	return PackedStringArray(["gpl"])


func _get_save_extension() -> String:
	return "tres"


func _get_resource_type() -> String:
	return "ColorPalette"


func _get_priority() -> float:
	return 1.0


func _get_preset_count() -> int:
	return 1


func _get_preset_name(preset_index: int) -> String:
	return "默认"


func _get_import_options(path: String, preset_index: int) -> Array:
	return []


func _import(
		source_file: String,
		save_path: String,
		options: Dictionary,
		platform_variants: Array,
		gen_files: Array,
) -> Error:
	if not FileAccess.file_exists(source_file):
		return ERR_FILE_NOT_FOUND
	var file_text := FileAccess.get_file_as_string(source_file)
	var parsed_colors := _parse_gpl_text(file_text)
	var palette := ColorPalette.new()
	palette.set_colors(parsed_colors)
	var destination_path := save_path + "." + _get_save_extension()
	return ResourceSaver.save(palette, destination_path)


func _parse_gpl_text(file_text: String) -> PackedColorArray:
	var result_colors := PackedColorArray()
	for raw_line in file_text.split("\n"):
		var line := raw_line.strip_edges()
		if line.is_empty():
			continue
		if line.to_lower() == "gimp palette":
			continue
		if line.begins_with("#"):
			continue
		var match_result := _rgb_line_pattern.search(line)
		if match_result == null:
			continue
		var red_channel := clampi(int(match_result.get_string(1)), 0, 255)
		var green_channel := clampi(int(match_result.get_string(2)), 0, 255)
		var blue_channel := clampi(int(match_result.get_string(3)), 0, 255)
		result_colors.append(
				Color(
						red_channel / 255.0,
						green_channel / 255.0,
						blue_channel / 255.0,
				)
		)
	return result_colors
