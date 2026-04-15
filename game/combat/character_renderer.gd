@tool
extends Control
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

var character: Character

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
	_refresh_layout_direction()
	position = _layout_target_position
	%AnimationPlayer.play(&"RESET")

func _process(delta: float) -> void:
	_refresh_renderer_size()
	var layout_target := _layout_target_position
	position = position.lerp(
		layout_target,
		clampf(POSITION_LERP_SPEED * delta, 0.0, 1.0),
	)

func bind(chr: Character) -> void:
	character = chr
	%Name.text = character.character_name
	var body_parts: Array[BodyPartRenderer] = [
		%Head, %Chest, %RightHand, %LeftHand, %RightFoot, %LeftFoot,
	]
	for index in body_parts.size():
		body_parts[index].setup(character.all_body_parts[index])
	%ActionPoints.bind(character.state_machine.action_points)
	_refresh_renderer_size()

func _notification(what: int) -> void:
	if what == NOTIFICATION_LAYOUT_DIRECTION_CHANGED:
		_refresh_layout_direction()

var _layout_target_position: Vector2:
	get: return active_position if centered else original_position

func _refresh_renderer_size() -> void:
	var minimun_size = %Container.get_combined_minimum_size()
	%Container.size = minimun_size
	size = minimun_size

func _refresh_layout_direction() -> void:
	var s = Vector2(-1, 1) if is_layout_rtl() else Vector2(1, 1)
	%Mirror.scale = s
	%Container.scale = s

signal deliver_hit

func _on_attack() -> void:
	deliver_hit.emit()

func animate_generic_attack() -> void:
	print("generic_attack")
	%AnimationPlayer.play(&"general_attack")
	await deliver_hit
	Engine.time_scale = 0
	%Timer.start(0.3)
	Engine.time_scale = 1
	await %Timer.timeout

func animate_generic_hit() -> void:
	%AnimationPlayer.play(&"general_hit")
