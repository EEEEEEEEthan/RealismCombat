extends Node

class_name Game

signal on_log(message: String)

var prepare: PrepareCanvas

func _ready() -> void:
	var prepare_scene = load(ResourceTable.SCENE_PREPARE_CANVAS)
	prepare = prepare_scene.instantiate()
	add_child(prepare)

func log(message) -> void:
	on_log.emit(str(message))
	print(message)
