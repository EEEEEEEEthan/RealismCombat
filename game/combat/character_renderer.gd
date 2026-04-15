@tool
extends PanelContainer
class_name CharacterRenderer

const POSITION_LERP_SPEED := 12.0
const POSITION_SETTLE_DISTANCE_SQUARED := 0.25

@onready var _expanded: Control = %Expanded

var original_position: Vector2 = Vector2.ZERO
var active_position: Vector2 = Vector2.ZERO

var centered: bool = false:
	set(value):
		if centered == value:
			return
		centered = value

@export var expanded: bool:
	set(value):
		expanded = value
		if is_node_ready():
			_expanded.expanded = expanded

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	anchor_left = 0.0
	anchor_top = 0.0
	anchor_right = 0.0
	anchor_bottom = 0.0
	offset_left = 0.0
	offset_top = 0.0
	offset_right = 0.0
	offset_bottom = 0.0
	set_process(true)
	_expanded.expanded = expanded
	_refresh_renderer_size()
	position = _layout_target_position()

func _process(delta: float) -> void:
	_refresh_renderer_size()
	var layout_target := _layout_target_position()
	position = position.lerp(
		layout_target,
		clampf(POSITION_LERP_SPEED * delta, 0.0, 1.0),
	)
	if _is_position_settled(position, layout_target):
		position = layout_target

func bind(character: Character) -> void:
	%Name.text = character.character_name
	var body_parts: Array[BodyPartRenderer] = [
		%Head, %Chest, %RightHand, %LeftHand, %RightFoot, %LeftFoot,
	]
	for index in body_parts.size():
		body_parts[index].setup(character.all_body_parts[index])
	%ActionPoints.bind(character.state_machine.action_points)
	_refresh_renderer_size()

func _layout_target_position() -> Vector2:
	return active_position if centered else original_position

func _refresh_renderer_size() -> void:
	size = get_combined_minimum_size()

static func _is_position_settled(pos: Vector2, target: Vector2) -> bool:
	return pos.distance_squared_to(target) <= POSITION_SETTLE_DISTANCE_SQUARED
