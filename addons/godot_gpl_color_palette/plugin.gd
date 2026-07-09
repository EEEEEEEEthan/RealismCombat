@tool
extends EditorPlugin

var _gpl_importer: EditorImportPlugin


func _enter_tree() -> void:
	_gpl_importer = preload("res://addons/godot_gpl_color_palette/gpl_importer_plugin.gd").new()
	add_import_plugin(_gpl_importer)


func _exit_tree() -> void:
	remove_import_plugin(_gpl_importer)
	_gpl_importer = null
