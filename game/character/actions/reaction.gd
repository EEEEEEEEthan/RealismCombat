class_name Reaction

enum Type
{
	Receive,
	Block,
	Dodge,
}

var type: Type
var block: RefCounted  # 如果是格挡，需要有一个东西用于格挡，可能是身体部位，也可能是装备

static var dodge_cost: float = 4
static var block_cost: float = 2

static func react(action: AttackAction, from_body: BodyPart, to_body: BodyPart) -> Reaction:
	var result = Reaction.new()
	result.type = Type.Receive
	if from_body.combat.characters[to_body.character] == 0:
		var execution_text = action.get_execution_text(from_body, to_body)
		var menu = Dialogues.create_menu_dialogue()
		menu.title = execution_text
		var options: Array[MenuItemData]
		var dodge_chance = action.get_dodge_chance(from_body, to_body)
		var doge_chance_text = &"(" + str(int(dodge_chance * 100)) + &"%)"
		if to_body.character.state_machine.current is CharacterStateMachineActionState:
			options.append(MenuItemData.new(&"格挡..", false, &"进行一次格挡,消耗行动力" + str(block_cost) + &",这会打断你当前的动作"))
			options.append(MenuItemData.new(&"闪避", false, &"进行一次闪避" + doge_chance_text + &",消耗行动力" + str(dodge_cost) + &",这会打断你当前的动作"))
		else:
			options.append(MenuItemData.new(&"格挡..", false, &"进行一次格挡,消耗行动力" + str(block_cost)))
			options.append(MenuItemData.new(&"闪避", false, &"进行一次闪避" + doge_chance_text + &",消耗行动力" + str(dodge_cost)))
		options.append(MenuItemData.new(&"硬抗", false, &"承受攻击"))
		menu.options = options
		while true:
			var choice = await menu.pressed
			if choice == 0:
				var block_body_parts: Array[BodyPart] = []
				var block_menu_options: Array[MenuItemData] = []
				for block_part: BodyPart in to_body.character.all_body_parts:
					block_menu_options.append(
						MenuItemData.new(str(block_part.part_name), false, &"用该部位格挡"),
					)
					block_body_parts.append(block_part)
				block_menu_options.append(MenuItemData.new(&"返回", false, &"返回上一级"))
				var block_menu = Dialogues.create_generic_dialogue()
				block_menu.title = execution_text + &">格挡"
				block_menu.options = block_menu_options
				var block_choice = await block_menu.pressed
				block_menu.queue_free()
				if block_choice < block_body_parts.size():
					result.type = Type.Block
					result.block = block_body_parts[block_choice]
					break
			elif choice == 1:
				result.type = Type.Dodge
				break
			else:
				result.type = Type.Receive
				break
		menu.queue_free()
	else:
		pass
		# todo: ai
	return result
