---
name: story-flow
overview: 在开局播放占位剧情对话并串联两段战斗，再进入现有菜单。
todos:
  - id: add-story-method
    content: 在Game添加剧情对白/战斗/菜单流程方法
    status: pending
  - id: enhance-generic-dialogue
    content: GenericDialogue新增Task<int>选择模式
    status: pending
  - id: task-flag-wait
    content: Game内添加剧情状态与穿胸甲flag序列化
    status: pending
  - id: add-breastplate
    content: 新增胸甲装备并接入初始/剧情流程
    status: pending
  - id: hook-story-start
    content: StartGameLoop开头调用剧情流程后进入菜单
    status: pending
---

# 开场剧情与选择/任务流程方案

- 在 [`Scripts/Game.cs`](Scripts/Game.cs) 新增开场剧情协程：对白 → 剧情菜单（可选择进入战斗）→ 战斗 → 收尾对白；流程可再次出现对白/菜单/战斗分段，剧情阶段可序列化当前段落/flag（含“穿上胸甲”任务）。对白节点需支持是/否分支，按玩家选择走不同台词/下一步。
- 在 [`Scripts/Nodes/Dialogues/GenericDialogue.cs`](Scripts/Nodes/Dialogues/GenericDialogue.cs) 增强：保留原API不变；新增返回 `Task<int>` 的选择模式（左右切换是/否并确认），仅在新API调用时显示选项。
- 在 `Game` 内提供剧情状态与任务flag序列化：记录当前剧情段、是否完成穿胸甲等；等待期间提示玩家操作，保存/读档后能恢复进度。
- 新增胸甲装备类型，放入初始物品栏或剧情奖励，便于完成任务并触发flag。
- 确保异常处理与原有保存/退出逻辑保持一致，剧情/任务/战斗流程结束后回到原有菜单。