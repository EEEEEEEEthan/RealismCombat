extends Node
class_name Game

signal on_log(message: String)
var prepare: PrepareCanvas
var command_handler: CommandHandler
var socket_server: SocketServer

func _init() -> void:
	command_handler = CommandHandler.new(self)
	socket_server = SocketServer.new(self)
	add_child(socket_server, false, Node.INTERNAL_MODE_BACK)

func _ready() -> void:
	var prepare_scene = load(ResourceTable.SCENE_PREPARE_CANVAS)
	prepare = prepare_scene.instantiate()
	add_child(prepare)

func log(message: Object) -> void:
	on_log.emit(str(message))
	print(message)

func exec_command(command: String) -> String:
	return command_handler.handle(command)
