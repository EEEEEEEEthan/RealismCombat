extends PanelContainer
class_name CharacterRenderer

var all_body_part_renderers: Array:
	get:
		if not all_body_part_renderers:
			all_body_part_renderers = [
				%Head, %Chest, %RightHand, %LeftHand, %RightFoot, %LeftFoot]
		return all_body_part_renderers

func setup(character: Character) -> void:
	%Name.text = character.character_name
	for i in 6:
		all_body_part_renderers[i].setup(character.all_body_parts[i])
