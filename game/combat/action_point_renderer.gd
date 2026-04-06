extends HBoxContainer
class_name ActionPointRenderer

var _action_points: Property
var _material: Material

func bind(action_points: Property) -> void:
	_action_points = action_points
	action_points.changed.connect(_on_value_changed)

func _ready() -> void:
	_material = $ProgressBar.material
	$ProgressBar.material = null

func _on_value_changed() -> void:
	$ProgressBar.value = _action_points.value
	if _action_points.value >= $ProgressBar.max_value:
		$ProgressBar.material = _material
	else:
		$ProgressBar.material = null
