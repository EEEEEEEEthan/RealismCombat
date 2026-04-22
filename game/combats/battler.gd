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
	character.head.hp.changed.connect(_on_vital_hp_changed)
	character.body.hp.changed.connect(_on_vital_hp_changed)


## 战斗结束时调用：解除与角色数据等的连接，避免长期持有回调。
func cleanup() -> void:
	if character.head.hp.changed.is_connected(_on_vital_hp_changed):
		character.head.hp.changed.disconnect(_on_vital_hp_changed)
	if character.body.hp.changed.is_connected(_on_vital_hp_changed):
		character.body.hp.changed.disconnect(_on_vital_hp_changed)


## 头或躯干 HP 变化时，若角色已死则立刻清空行动力（与存活判定一致）。
func _on_vital_hp_changed() -> void:
	if character.alive:
		return
	state_machine.action_points.value = 0
