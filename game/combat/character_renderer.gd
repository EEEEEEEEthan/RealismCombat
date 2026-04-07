extends PanelContainer
class_name CharacterRenderer

var all_body_part_renderers: Array[BodyPartRenderer]:
	get:
		if not all_body_part_renderers:
			all_body_part_renderers = [
				%Head, %Chest, %RightHand, %LeftHand, %RightFoot, %LeftFoot]
		return all_body_part_renderers

func bind(character: Character) -> void:
	%Name.text = character.character_name
	for i in 6:
		all_body_part_renderers[i].setup(character.all_body_parts[i])
	%ActionPoints.bind(character.action_points)
