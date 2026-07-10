# GDScript (Godot 4) 补充说明

## 优化检查清单

- [ ] 仅被引用一次的计算属性（`var x: get: return ...`）应内联到调用处
- [ ] `@export` 属性的 setter 是唯一触发更新的入口，不要在别处重复调用 `_update_xxx`
- [ ] 简单信号处理器（仅一行逻辑）应内联为 lambda：`signal.connect(func(): ...)`
- [ ] 不依赖 `self` 的工具方法标记为 `static func`
- [ ] `@tool` 脚本中避免在 `_init` 做重操作；初始化逻辑用 `_ready` + `is_node_ready()` 守卫
- [ ] `ResourceLoader.load()` 结果若多处使用应缓存为成员或 static 变量
- [ ] 编译期确定的资源用 `preload("uid://...")` 替代 `ResourceLoader.load()`
- [ ] 移除调试用的 `print()` 语句
- [ ] `@onready var node := $Path` 用于节点引用
- [ ] 使用 `Node.INTERNAL_MODE_FRONT/BACK` 添加内部子节点，避免序列化到场景
- [ ] `_exit_tree` 中清空成员引用以辅助 GC
- [ ] 类型注解 `: Type` 应始终标注（参数、返回值、变量）
- [ ] 命名字段避免匈牙利前缀（如 `_cached_xxx`），直接用语义名
