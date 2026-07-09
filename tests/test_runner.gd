extends Node

const TestSmoke := preload("res://tests/regression/test_smoke.gd")
const _Common := preload("res://tests/regression/_common.gd")

const _ARG_PREFIX := "--regression-test="
const _SUITE_TIMEOUT_SEC := 60.0


func _ready() -> void:
	var test_spec := _read_test_spec()
	if test_spec.is_empty():
		return
	print("运行回归测试: %s" % test_spec)
	await get_tree().process_frame
	var outcome := await _run_with_suite_timeout(test_spec)
	_print_outcome(test_spec, outcome)
	await _Common.flush_frames(get_tree(), 8)
	get_tree().quit(0 if outcome.get("passed", false) else 1)


func _read_test_spec() -> String:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with(_ARG_PREFIX):
			return argument.substr(_ARG_PREFIX.length())
	return ""


func _run_with_suite_timeout(test_spec: String) -> Dictionary:
	var state := {"completed": false, "outcome": {}}
	_collect_spec_outcome(test_spec, state)
	var elapsed_sec := 0.0
	while not state.completed and elapsed_sec < _SUITE_TIMEOUT_SEC:
		await get_tree().process_frame
		elapsed_sec += get_process_delta_time()
	if not state.completed:
		return {
			"passed": false,
			"steps": [],
			"error": "测试套件超时 (%.0fs)" % _SUITE_TIMEOUT_SEC,
		}
	if not state.outcome.has("passed"):
		return {
			"passed": false,
			"steps": [],
			"error": "测试运行异常",
		}
	return state.outcome


func _collect_spec_outcome(test_spec: String, state: Dictionary) -> void:
	state.outcome = await _run_spec(test_spec)
	state.completed = true


func _run_spec(test_spec: String) -> Dictionary:
	match test_spec:
		"smoke":
			return await _run_single("smoke", TestSmoke.run)
		_:
			return {
				"passed": false,
				"steps": [],
				"error": "未知测试: %s" % test_spec,
			}


func _run_single(test_name: String, runner: Callable) -> Dictionary:
	print("运行测试: %s" % test_name)
	var outcome: Dictionary = await runner.call(get_tree())
	if not outcome.has("steps"):
		outcome["steps"] = []
	if not outcome.has("error"):
		outcome["error"] = ""
	if not outcome.has("passed"):
		outcome["passed"] = false
	return outcome


func _print_outcome(test_spec: String, outcome: Dictionary) -> void:
	print("测试套件: %s" % test_spec)
	var steps: Array = outcome.get("steps", [])
	for entry in steps:
		if not entry is Dictionary:
			continue
		var marker := "✓" if entry.get("ok") else "✗"
		var step_name: String = entry.get("step", "?")
		var detail: String = entry.get("detail", "")
		print("  %s %s: %s" % [marker, step_name, detail])
	if outcome.get("passed", false):
		print("回归测试通过")
	else:
		print("回归测试失败: %s" % outcome.get("error", "未知错误"))
