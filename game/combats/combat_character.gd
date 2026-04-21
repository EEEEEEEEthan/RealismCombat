extends RefCounted
class_name CombatCharacter
# 战斗角色。生命周期是战斗开始add_character到战斗结束。
# 胶水代码，属性数据转发给角色。战斗特有的数据(buff,状态机等,自己处理)

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
