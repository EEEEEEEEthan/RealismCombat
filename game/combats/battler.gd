extends RefCounted
class_name Battler
# 战斗中的参战单位。生命周期：Combat.add_character 至战斗结束。
# 胶水：属性转发 raw_character；状态机等战斗特有数据在此处理。

var raw_character: Character
var state_machine: CharacterStateMachine
var combat: Combat

var game: Game:
	get: return raw_character.game
var character_name: String:
	get: return raw_character.character_name
var head: Head:
	get: return raw_character.head
var body: Body:
	get: return raw_character.body
var right_hand: Hand:
	get: return raw_character.right_hand
var left_hand: Hand:
	get: return raw_character.left_hand
var right_foot: Foot:
	get: return raw_character.right_foot
var left_foot: Foot:
	get: return raw_character.left_foot
var all_body_parts: Array[BodyPart]:
	get: return raw_character.all_body_parts
var actions: Array[Action]:
	get: return raw_character.actions
var alive: bool:
	get: return raw_character.alive
var speed: float:
	get: return raw_character.speed

func _init(p_combat: Combat, character: Character) -> void:
	raw_character = character
	combat = p_combat
	state_machine = CharacterStateMachine.new(self)
