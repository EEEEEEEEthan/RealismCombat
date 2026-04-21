extends RefCounted
class_name Battler
# 战斗中的参战单位。生命周期：Combat.add_character 至战斗结束。
# 胶水：属性转发 character；状态机等战斗特有数据在此处理。

var character: Character
var state_machine: CharacterStateMachine
var combat: Combat

var game: Game:
	get: return character.game
var character_name: String:
	get: return character.character_name
var head: Head:
	get: return character.head
var body: Body:
	get: return character.body
var right_hand: Hand:
	get: return character.right_hand
var left_hand: Hand:
	get: return character.left_hand
var right_foot: Foot:
	get: return character.right_foot
var left_foot: Foot:
	get: return character.left_foot
var all_body_parts: Array[BodyPart]:
	get: return character.all_body_parts
var actions: Array[Action]:
	get: return character.actions
var alive: bool:
	get: return character.alive
var speed: float:
	get: return character.speed

func _init(p_combat: Combat, p_character: Character) -> void:
	character = p_character
	combat = p_combat
	state_machine = CharacterStateMachine.new(self)
