# 用 `%` 取子节点却得到 null

## 背景

`CharacterStateMachine` 通过 `@onready var current_state: Node = %IdleState` 绑定当前状态；场景来自 `character_state_machine.tscn`，经 `InstancePlaceholder.create_instance()` 挂到角色下。

## 现象

运行时错误：`Attempt to call function 'new_tick' in base 'null instance'`，栈指向 `character_state_machine.gd` 中 `await current_state.new_tick()`。

## 根因

Godot 4 中 `%NodeName`（`get_node("%NodeName")`）只解析在**所属场景**里勾选了「场景唯一名称」（`unique_name_in_owner = true`）的节点。`IdleState` 子节点未勾选时，解析结果为 `null`，`@onready` 会把 `current_state` 设为 `null`。

## 正确做法

- 凡在脚本里用 `%` 引用的节点，在对应 `.tscn` 中对该节点启用场景唯一名称；或改用 `$IdleState` 等路径（不依赖唯一名）。
- 新增子节点若要用 `%`，记得同步勾选，避免只改了脚本忘改场景。

## 相关文件

- `game/combat/character_state_machine.gd`（`current_state`）
- `game/combat/character_state_machine.tscn`（为 `IdleState` 增加 `unique_name_in_owner = true`）
