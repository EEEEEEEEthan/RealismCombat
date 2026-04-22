extends HBoxContainer
class_name ActionPointRenderer

var _action_points: Property
var _material: Material

func bind(action_points: Property) -> void:
	if _action_points != null and _action_points.changed.is_connected(_on_value_changed):
		_action_points.changed.disconnect(_on_value_changed)
	_action_points = action_points
	_action_points.changed.connect(_on_value_changed)


func _exit_tree() -> void:
	if _action_points != null and _action_points.changed.is_connected(_on_value_changed):
		_action_points.changed.disconnect(_on_value_changed)
	_action_points = null

func _ready() -> void:
	_material = $ProgressBar.material
	$ProgressBar.material = null

func _on_value_changed() -> void:
	$ProgressBar.value = _action_points.value
	$ProgressBar.jump = _action_points.value >= $ProgressBar.max_value
