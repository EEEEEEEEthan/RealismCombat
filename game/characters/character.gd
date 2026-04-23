extends RefCounted
class_name Character
# 角色数据模型：身体部位、HP、动作列表、存活与速度等聚合。
# 不与战斗耦合

var id: int
var game: Game
var character_name: String
var head: Head
var body: Body
var right_hand: Hand
var left_hand: Hand
var right_foot: Foot
var left_foot: Foot
var all_body_parts: Array[BodyPart]
var _actions: Array[Action] = []

var actions: Array[Action]:
	get: return _actions
var alive: bool:
	get: return head.hp.value > 0 and body.hp.value > 0
var speed: float:
	get: return 10

static func create_deserialize(p_game: Game, file_access: FileAccess) -> Character:
	var c = Character.new(p_game)
	c._deserialize(file_access)
	return c

## 默认装备：罩袍外套、左右皮鞋与皮手套。
static func create_default(p_game: Game, p_name: String) -> Character:
	var c := Character.new(p_game)
	c.id = p_game.next_object_id
	c.character_name = p_name
	c.body.torso_slot.item = SurcoatTabard.new()
	c.right_hand.glove_slot.item = LeatherGlove.new(Defs.Side.RIGHT)
	c.left_hand.glove_slot.item = LeatherGlove.new(Defs.Side.LEFT)
	c.right_foot.footwear_slot.item = LeatherBoot.new(Defs.Side.RIGHT)
	c.left_foot.footwear_slot.item = LeatherBoot.new(Defs.Side.LEFT)
	return c

func _init(p_game: Game) -> void:
	game = p_game
	head = Head.new(self, 3, 3)
	body = Body.new(self, 10, 10)
	right_hand = Hand.new(self, 6, 6, Defs.Side.RIGHT)
	left_hand = Hand.new(self, 6, 6, Defs.Side.LEFT)
	right_foot = Foot.new(self, 7, 7, Defs.Side.RIGHT)
	left_foot = Foot.new(self, 7, 7, Defs.Side.LEFT)
	all_body_parts = [ head, body, right_hand, left_hand, right_foot, left_foot, ]
	add_action(AttackActionPunch.new(self))
	add_action(AttackActionKick.new(self))

func add_action(action: Action) -> void:
	_actions.append(action)

func serialize(file_access: FileAccess) -> void:
	file_access.store_64(id)
	file_access.store_pascal_string(character_name)
	for part: BodyPart in all_body_parts:
		part.serialize(file_access)

func _deserialize(file_access: FileAccess) -> void:
	id = file_access.get_64()
	character_name = file_access.get_pascal_string()
	for part: BodyPart in all_body_parts:
		part.deserialize(file_access)
