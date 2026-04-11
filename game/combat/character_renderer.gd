@tool
extends PanelContainer
class_name CharacterRenderer

@export var expanded: bool:
	set(value):
		expanded = value
		if is_node_ready():
			%Expanded.expanded = expanded

func _ready() -> void:
	%Expanded.expanded = expanded

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
	%ActionPoints.bind(character.state_machine.action_points)
