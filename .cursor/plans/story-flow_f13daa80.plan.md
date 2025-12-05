---
name: story-flow
overview: 实现贯穿游戏的剧情流程：对白 → GameMenu → 战斗 循环，对白中可以等待flag。
todos:
  - id: design-story-data-structure
    content: 设计剧情数据结构：StoryNode类型（对白/菜单/战斗/等待flag）、分支支持、StoryState状态管理
    status: pending
  - id: add-story-method
    content: 在Game添加剧情对白/战斗/菜单流程方法
    status: pending
  - id: enhance-generic-dialogue
    content: GenericDialogue新增Task<int>选择模式
    status: completed
  - id: task-flag-wait
    content: Game内添加剧情状态与穿胸甲flag序列化
    status: pending
  - id: add-breastplate
    content: 新增胸甲装备并接入初始/剧情流程
    status: pending
  - id: hook-story-start
    content: 在游戏流程中接入剧情循环
    status: pending
---

# 贯穿游戏的剧情与选择/任务流程方案

- **数据结构设计**：设计剧情节点系统，包括：
- `StoryNode` 类型枚举：对白（Dialogue）、菜单（Menu）、战斗（Combat）、等待flag（WaitFlag）
- 对白节点支持文本、分支选择（选择后跳转到不同节点）
- 等待flag节点：等待指定flag被设置后继续
- `StoryState`：管理当前剧情节点索引、flag集合、序列化支持
- 在 [`Scripts/Game.cs`](Scripts/Game.cs) 新增贯穿游戏的剧情协程：对白 → GameMenu → 战斗 循环；对白中可以等待flag（如"穿上胸甲"任务），剧情阶段可序列化当前段落/flag。对白节点需支持分支，按玩家选择走不同台词/下一步。
- 在 [`Scripts/Nodes/Dialogues/GenericDialogue.cs`](Scripts/Nodes/Dialogues/GenericDialogue.cs) 增强：保留原API不变；新增返回 `Task<int>` 的选择模式（左右切换是/否并确认），仅在新API调用时显示选项。
- 在 `Game` 内提供剧情状态与任务flag序列化：记录当前剧情段、是否完成穿胸甲等；等待期间提示玩家操作，保存/读档后能恢复进度。
- 新增胸甲装备类型，放入初始物品栏或剧情奖励，便于完成任务并触发flag。
- 确保异常处理与原有保存/退出逻辑保持一致，剧情/任务/战斗流程结束后回到原有菜单。