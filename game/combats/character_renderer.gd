@tool
extends Control
class_name CharacterRenderer

const POSITION_LERP_SPEED := 12.0
const POSITION_SETTLE_DISTANCE_SQUARED := 0.25

@onready var _expanded: Control = %Expanded

var original_position: Vector2 = Vector2.ZERO
var active_position: Vector2 = Vector2.ZERO
var body_parts: Array[BodyPartRenderer]:
	get:
		if not body_parts:
			body_parts = [ %Head, %Body, %RightHand, %LeftHand, %RightFoot, %LeftFoot, ]
		return body_parts

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

var _override_color: Defs.ColorFamily = Defs.ColorFamily.NONE

var _color: Defs.ColorFamily:
	get:
		if _override_color != Defs.ColorFamily.NONE:
			return _override_color
		if not character:
			return Defs.ColorFamily.NEUTRAL
		if character.alive:
			if is_layout_rtl():
				return Defs.ColorFamily.FORGE_EMBER
			else:
				return Defs.ColorFamily.OCEAN_BLUE
		return Defs.ColorFamily.NEUTRAL

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
	_refresh_color()

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
	assert(len(body_parts) > 0)
	for index in body_parts.size():
		body_parts[index].setup(character.all_body_parts[index])
	%ActionPoints.bind(character.state_machine.action_points)
	_refresh_renderer_size()

func _notification(what: int) -> void:
	if what == NOTIFICATION_LAYOUT_DIRECTION_CHANGED:
		_refresh_layout_direction()
		_refresh_color()

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

func _refresh_color() -> void:
	if not character: return
	var color := _color
	%Container.fill_color_family = color
	%ActionPoints.color_family = color
	for part: BodyPartRenderer in body_parts:
		part.color_family = color

signal _deliver_hit

func _on_attack() -> void:
	_deliver_hit.emit()

func animate_generic_attack() -> void:
	%AnimationPlayer.play(&"general_attack")
	await _deliver_hit

func animate_generic_hit() -> void:
	%AnimationPlayer.play(&"general_hit")

func animate_heavy_hit() -> void:
	_override_color = Defs.ColorFamily.ROSE_PINK
	_refresh_color()
	var ap := %AnimationPlayer
	ap.play(&"general_hit")
	await ap.animation_finished
	_override_color = Defs.ColorFamily.NONE
	_refresh_color()

func animate_generic_dodge() -> void:
	%AnimationPlayer.play(&"generic_dodge")
