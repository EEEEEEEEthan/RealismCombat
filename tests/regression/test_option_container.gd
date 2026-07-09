## 回归测试：OptionContainer 上下箭头视口滚动行为
extends RefCounted

const _Common := preload("res://tests/regression/_common.gd")

const CONTENT_COUNT := 5


static func run(scene_tree: SceneTree) -> Dictionary:
	var steps: Array[Dictionary] = []

	# ---------------------------------------------------------------
	# 1. 创建 OptionContainer 实例，设置 viewport_size=3，添加 5 个 Label
	# ---------------------------------------------------------------
	var container := _build_container(3, CONTENT_COUNT)
	scene_tree.root.add_child(container)
	await _Common.flush_frames(scene_tree, 2)

	var up: TextureButton = container.get_node(NodePath("UpArrow"))
	var down: TextureButton = container.get_node(NodePath("DownArrow"))

	# ---------------------------------------------------------------
	# 2. 验证初始状态 (viewport_begin=0)
	#    viewport_size=3，下箭头占 1 槽位 ⇒ 2 个内容槽位
	# ---------------------------------------------------------------
	_assert_arrows(steps, "初始", up, false, down, true)

	# 第 0、1 个 Label 可见
	_assert_labels(steps, "初始", container, func(i: int) -> bool: return i < 2)

	# ---------------------------------------------------------------
	# 3. viewport_begin=1：上箭头占 1 槽 + 下箭头占 1 槽 ⇒ 1 个内容槽
	# ---------------------------------------------------------------
	container.viewport_begin = 1
	await _Common.flush_frames(scene_tree, 1)

	_assert_arrows(steps, "step1", up, true, down, true)

	# 仅 Label1 可见
	_assert_labels(steps, "step1", container, func(i: int) -> bool: return i == 1)

	# ---------------------------------------------------------------
	# 4. viewport_begin=3（末尾）：上箭头显示、下箭头隐藏、最后 2 个 Label 可见
	# ---------------------------------------------------------------
	container.viewport_begin = 3
	await _Common.flush_frames(scene_tree, 1)

	_assert_arrows(steps, "end", up, true, down, false)

	# 最后 2 个 Label（索引 3、4）可见
	_assert_labels(steps, "end", container, func(i: int) -> bool: return i >= 3)

	# ---------------------------------------------------------------
	# 5. viewport_begin=0, viewport_size=5（足够容纳全部）
	# ---------------------------------------------------------------
	container.viewport_begin = 0
	container.viewport_size = CONTENT_COUNT
	await _Common.flush_frames(scene_tree, 1)

	_assert_arrows(steps, "全显", up, false, down, false)

	# 全部 Label 可见
	_assert_labels(steps, "全显", container, func(_i: int) -> bool: return true)

	# ---------------------------------------------------------------
	# 6. 边界：content_count=1 时 viewport_begin=0 应正常工作
	# ---------------------------------------------------------------
	var small_container := _build_container(3, 1)
	scene_tree.root.add_child(small_container)
	await _Common.flush_frames(scene_tree, 2)

	var small_up: TextureButton = small_container.get_node(NodePath("UpArrow"))
	var small_down: TextureButton = small_container.get_node(NodePath("DownArrow"))

	_assert_arrows(steps, "边界", small_up, false, small_down, false)

	var single_label := small_container.get_child(0, false) as Label
	var passed := single_label.visible
	steps.append(_Common.step(passed, "边界-唯一Label可见",
		"唯一 Label visible=%s (期望=true)" % single_label.visible))

	# 清理
	small_container.queue_free()

	# ---------------------------------------------------------------
	# 7. 清理主测试容器
	# ---------------------------------------------------------------
	container.queue_free()
	await _Common.flush_frames(scene_tree, 1)

	return _Common.result(true, steps, "")


## 构建指定 size 和内容数量的 OptionContainer（内容为 Label）
static func _build_container(viewport_size: int, content_count: int) -> OptionContainer:
	var container := OptionContainer.new()
	container.viewport_size = viewport_size
	for i in content_count:
		var label := Label.new()
		label.name = "Label%d" % i
		label.text = "Item %d" % i
		container.add_child(label)
	return container


## 断言上下箭头的可见性并记录步骤
static func _assert_arrows(
	steps: Array[Dictionary],
	prefix: String,
	up: TextureButton,
	up_visible: bool,
	down: TextureButton,
	down_visible: bool,
) -> void:
	var passed := up.visible == up_visible
	var detail := "上箭头%s" % ("显示" if up_visible else "隐藏")
	if not passed:
		detail += " (实际=%s)" % up.visible
	steps.append(_Common.step(passed, "%s-上箭头%s" % [prefix, "显示" if up_visible else "隐藏"], detail))

	passed = down.visible == down_visible
	detail = "下箭头%s" % ("显示" if down_visible else "隐藏")
	if not passed:
		detail += " (实际=%s)" % down.visible
	steps.append(_Common.step(passed, "%s-下箭头%s" % [prefix, "显示" if down_visible else "隐藏"], detail))


## 断言所有子 Label 的可见性符合 predicate 并记录步骤
static func _assert_labels(
	steps: Array[Dictionary],
	prefix: String,
	container: OptionContainer,
	predicate: Callable,
) -> void:
	var content_count := container.get_child_count(false)
	for i in content_count:
		var lbl := container.get_child(i, false) as Label
		var expect_visible: bool = predicate.call(i)
		var passed: bool = lbl.visible == expect_visible
		var detail := "Label%d visible=%s (期望=%s)" % [i, lbl.visible, expect_visible]
		steps.append(_Common.step(passed, "%s-Label%d" % [prefix, i], detail))
