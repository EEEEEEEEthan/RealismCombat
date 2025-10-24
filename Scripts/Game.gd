extends Node
class_name Game

signal on_log(message: String)
var prepare_canvas: PrepareCanvas
var command_handler: CommandHandler
var socket_server: SocketServer

func _init() -> void:
	command_handler = CommandHandler.new(self)
	socket_server = SocketServer.new(self)
	add_child(socket_server, false, Node.INTERNAL_MODE_BACK)

func _ready() -> void:
	var scene = load(ResourceTable.SCENE_PREPARE_CANVAS)
	prepare_canvas = scene.instantiate()
	add_child(prepare_canvas)

func log(message: Object) -> void:
	on_log.emit(str(message))
	print(message)

func exec_command(command: String) -> String:
	return command_handler.handle(command)
