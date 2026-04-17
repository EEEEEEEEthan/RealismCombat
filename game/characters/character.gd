extends RefCounted
class_name Character

var game: Game
var character_name: String
var head: Head
var chest: Chest
var right_hand: Hand
var left_hand: Hand
var right_foot: Foot
var left_foot: Foot
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
	head = Head.new(self, 3, 3)
	chest = Chest.new(self, 10, 10)
	right_hand = Hand.new(self, 6, 6, Defs.Side.RIGHT)
	left_hand = Hand.new(self, 6, 6, Defs.Side.LEFT)
	right_foot = Foot.new(self, 7, 7, Defs.Side.RIGHT)
	left_foot = Foot.new(self, 7, 7, Defs.Side.LEFT)
	all_body_parts = [ head, chest, right_hand, left_hand, right_foot, left_foot, ]
	add_action(AttackActionPunch.new(self))

func add_action(action: Action) -> void:
	_actions.append(action)

func on_enter_combat() -> void:
	state_machine = CharacterStateMachine.new(self)

func on_exit_combat() -> void:
	state_machine = null
