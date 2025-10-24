extends Node
class_name Game

static var instance: Game

signal on_log(message: String)
var prepare: PrepareCanvas
var _command_handler: CommandHandler = CommandHandler.new()

func _init() -> void:
	instance = self

func _ready() -> void:
	var prepare_scene = load(ResourceTable.SCENE_PREPARE_CANVAS)
	prepare = prepare_scene.instantiate()
	add_child(prepare)

func log(message: Object) -> void:
	on_log.emit(str(message))
	print(message)

func exec_command(command: String) -> String:
	return _command_handler.handle(command)
