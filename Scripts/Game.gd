extends Node

class_name Game

signal on_log(message: String)

@export var prerpare: PrepareCanvas

func log(message) -> void:
	on_log.emit(str(message))
	print(message)
