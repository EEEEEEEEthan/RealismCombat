extends RefCounted

const WAIT_SLICE_SEC := 0.05


static func flush_frames(scene_tree: SceneTree, frame_count: int = 3) -> void:
	for _flush_index in frame_count:
		await scene_tree.process_frame


static func wait_until(scene_tree: SceneTree, condition: Callable, timeout_sec: float) -> bool:
	var timer := Timer.new()
	timer.process_mode = Node.PROCESS_MODE_ALWAYS
	timer.wait_time = WAIT_SLICE_SEC
	scene_tree.root.add_child(timer)
	timer.start()
	var elapsed_sec := 0.0
	while elapsed_sec < timeout_sec:
		if condition.call():
			timer.queue_free()
			return true
		await timer.timeout
		elapsed_sec += WAIT_SLICE_SEC
	timer.queue_free()
	return false


static func step(passed: bool, step_name: String, detail: String) -> Dictionary:
	return {"ok": passed, "step": step_name, "detail": detail}


static func result(passed: bool, steps: Array[Dictionary], error: String) -> Dictionary:
	return {"passed": passed, "steps": steps, "error": error}


static func fail(steps: Array[Dictionary], error: String) -> Dictionary:
	return result(false, steps, error)
