## 冒烟测试：启动游戏，确认主场景加载后结束。
extends RefCounted

const _Common := preload("res://tests/regression/_common.gd")


static func run(scene_tree: SceneTree) -> Dictionary:
	var steps: Array[Dictionary] = []
	await scene_tree.process_frame
	var main_scene := scene_tree.current_scene
	if main_scene == null:
		return _Common.fail(steps, "主场景未加载")
	steps.append(_Common.step(true, "启动", "主场景 %s 已加载" % main_scene.name))
	await _Common.flush_frames(scene_tree, 3)
	return _Common.result(true, steps, "")
