extends Node

## 焦点调试器
## 监听所有控件的focus事件并打印路径

var _已连接的控件: Array[Control] = []

func _ready() -> void:
	# 等待场景树完全就绪后再连接信号
	call_deferred("_连接所有控件的焦点信号")
	
	# 使用定时器定期检查新添加的节点（用于捕获动态创建的节点）
	var timer = Timer.new()
	timer.wait_time = 0.5  # 每0.5秒检查一次
	timer.timeout.connect(_检查新节点)
	timer.autostart = true
	add_child(timer)

func _连接所有控件的焦点信号() -> void:
	_遍历并连接(get_tree().root)

func _遍历并连接(node: Node) -> void:
	# 如果是Control节点，连接焦点信号
	if node is Control:
		var control = node as Control
		# 检查是否已经连接过（避免重复连接）
		if control not in _已连接的控件:
			control.focus_entered.connect(_on_focus_entered.bind(control))
			control.focus_exited.connect(_on_focus_exited.bind(control))
			_已连接的控件.append(control)
	
	# 递归遍历所有子节点
	for child in node.get_children():
		_遍历并连接(child)

func _检查新节点() -> void:
	# 检查场景树中是否有新的Control节点需要连接
	_遍历并连接(get_tree().root)

func _on_focus_entered(control: Control) -> void:
	# 打印控件的路径
	print("[焦点调试] 控件获得焦点: ", control.get_path())

func _on_focus_exited(control: Control) -> void:
	# 打印控件的路径
	print("[焦点调试] 控件失去焦点: ", control.get_path())
