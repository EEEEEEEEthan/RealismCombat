@tool
extends PanelContainer
class_name CharacterRenderer

const POSITION_LERP_SPEED := 12.0
const POSITION_SETTLE_DISTANCE_SQUARED := 0.25

signal preferred_position_reached
signal centered_changed

var _is_at_preferred_position := true
@onready var _expanded: Control = %Expanded

var preferred_position: Vector2 = Vector2.ZERO:
	set(value):
		preferred_position = value
		if not _is_position_settled(position, preferred_position):
			_is_at_preferred_position = false

var centered: bool = false:
	set(value):
		if centered == value:
			return
		centered = value
		centered_changed.emit()

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
	position = preferred_position
	_is_at_preferred_position = true

func _process(delta: float) -> void:
	_refresh_renderer_size()
	position = position.lerp(
		preferred_position,
		clampf(POSITION_LERP_SPEED * delta, 0.0, 1.0),
	)
	if _is_position_settled(position, preferred_position):
		position = preferred_position
		if not _is_at_preferred_position:
			_is_at_preferred_position = true
			preferred_position_reached.emit()
	else:
		_is_at_preferred_position = false

func bind(character: Character) -> void:
	%Name.text = character.character_name
	var body_parts: Array[BodyPartRenderer] = [
		%Head, %Chest, %RightHand, %LeftHand, %RightFoot, %LeftFoot,
	]
	for i in body_parts.size():
		body_parts[i].setup(character.all_body_parts[i])
	%ActionPoints.bind(character.state_machine.action_points)
	_refresh_renderer_size()

func wait_until_preferred_position() -> void:
	if _is_position_settled(position, preferred_position):
		position = preferred_position
		_is_at_preferred_position = true
		return
	await preferred_position_reached

func _refresh_renderer_size() -> void:
	size = get_combined_minimum_size()

static func _is_position_settled(pos: Vector2, pref: Vector2) -> bool:
	return pos.distance_squared_to(pref) <= POSITION_SETTLE_DISTANCE_SQUARED
