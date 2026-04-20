extends RefCounted
class_name EquipmentNavigationStack

## 装备子菜单「返回」用的逻辑栈：从角色列表 drill-down 到身体部位、槽位容器、嵌套物品槽。
## 栈帧具体形状由 EquipmentMenuFlow 落地时确定（建议用显式枚举或小型 RefCounted 帧类型，避免用裸 Dictionary 长期扩散）。


# TODO: 用 Array 或 TypedArray 持有栈帧；depth 返回当前层数，供菜单标题或调试；空栈时返回 0
func depth() -> int:
	push_error("EquipmentNavigationStack.depth：未实现")
	return 0


# TODO: 进入装备子菜单根或完全退出流程时调用；须释放帧内引用（Character/BodyPart/Item 指针）避免悬挂
func clear() -> void:
	push_error("EquipmentNavigationStack.clear：未实现")


# TODO: 进入下一层菜单前压栈；帧内需能区分「角色列表 / 身体部位 / 某身体部位槽列表 / 某物品槽列表」以便 pop 后恢复 UI 状态；与 EquipmentMenuFlow 的「返回」索引约定一致
func push(_frame: Variant) -> void:
	push_error("EquipmentNavigationStack.push：未实现")


# TODO: 用户选「返回」时弹出；空栈时返回 null 或断言，由调用方决定是否等价于退出装备菜单；嵌套物品场景下不得只 pop 一半导致导航不一致
func pop() -> Variant:
	push_error("EquipmentNavigationStack.pop：未实现")
	return null
