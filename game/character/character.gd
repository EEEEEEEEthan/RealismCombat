extends RefCounted
class_name Character

var game: Game
var character_name: String
var head: BodyPart
var chest: BodyPart
var right_hand: BodyPart
var left_hand: BodyPart
var right_foot: BodyPart
var left_foot: BodyPart
var all_body_parts: Array[BodyPart]
var state_machine: CharacterStateMachine
var _actions: Array[Action] = []

var actions: Array[Action]:
	get: return _actions

var alive: bool:
	get: return head.hp.value > 0 and chest.hp.value > 0

var speed: float:
	get: return 10

func _init(p_game: Game, p_name: String) -> void:
	game = p_game
	character_name = p_name
	head = BodyPart.new(self, Defs.BodyPart.HEAD, 3, 3)
	chest = BodyPart.new(self, Defs.BodyPart.CHEST, 10, 10)
	right_hand = BodyPart.new(self, Defs.BodyPart.RIGHT_HAND, 6, 6)
	left_hand = BodyPart.new(self, Defs.BodyPart.LEFT_HAND, 6, 6)
	right_foot = BodyPart.new(self, Defs.BodyPart.RIGHT_FOOT, 7, 7)
	left_foot = BodyPart.new(self, Defs.BodyPart.LEFT_FOOT, 7, 7)
	all_body_parts = [ head, chest, right_hand, left_hand, right_foot, left_foot, ]
	add_action(AttackActionPunch.new(self))

func add_action(action: Action) -> void:
	_actions.append(action)

func on_enter_combat() -> void:
	state_machine = CharacterStateMachine.new(self)

func on_exit_combat() -> void:
	state_machine = null
