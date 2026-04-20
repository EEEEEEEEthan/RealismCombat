extends RefCounted
class_name EquipmentNavigationStack

## 装备子菜单导航栈：向下进入子菜单时 push，子菜单返回时 pop，用于调试或后续面包屑标题。

var _frames: Array = []


func depth() -> int:
	return _frames.size()


func clear() -> void:
	_frames.clear()


func push(frame: Variant) -> void:
	_frames.append(frame)


func pop() -> Variant:
	if _frames.is_empty():
		return null
	return _frames.pop_back()


func peek() -> Variant:
	if _frames.is_empty():
		return null
	return _frames[_frames.size() - 1]
