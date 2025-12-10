# 游戏流程（Game）

## 设计目标
- 用单一的 `Game` 类串联剧情、装备管理与战斗入口，保持脚本线性可读
- 依赖对话框系统和 MCP 检查点，保证本地游玩与远程驱动体验一致
- 使用自定义等待器 (`await game`) 统一异步生命周期，便于在节点上托管

## 核心结构
- `ScriptCode`：章节枚举，当前包含 `_0_Intro`、`_1_Equip`、`_2_Wander`
- 成员字段：
  - `players`：玩家队伍列表，新游戏默认只有 `Ethan`
  - `gameNode`：承载场景的 Godot 节点，用于挂载战斗界面
  - `saveFilePath`：存档路径，允许空路径跳过写盘
  - `taskCompletionSource`：封装 `Game` 生命周期的等待器
  - `ScriptIndex`：当前脚本进度，读档时从文件恢复
- 序列化：
  - `GetSnapshot()` 写入版本信息，随后序列化玩家列表与 `ScriptIndex`
  - 读档构造函数会先读取 `Snapshot` 再加载 `players` 与进度

## 生命周期
- 新游戏：构造时创建 `Ethan`，将 `CottonLiner`、`CottonPants`、`Belt` 穿戴好，同时在物品栏放入 `LongSword`、`ChainMail`、`ChainChausses`，随后启动主循环
- 读取存档：接受 `BinaryReader`，恢复玩家与脚本进度后启动主循环
- 退出：`Quit()` 先保存再完成等待器，外部可通过 `await game` 得知结束

## 主循环阶段
### _0_Intro 叙事
- 使用 `GenericDialogue` 展示背景故事，末尾将 `ScriptIndex` 推进到装备章节

### _1_Equip 装备章节
- 菜单项：`走吧...`、`装备`、`存档`、`退出游戏`
- 出发条件：主角需已装备长剑、链甲、链甲护腿，未满足时禁用“走吧...”
- 交互：
  - `装备` 进入装备管理流程
  - `存档` 立即写盘并提示
  - `退出游戏` 调用 `Quit()` 返回主菜单
- 满足条件且选择“走吧...”后切换到 `_2_Wander`

### _2_Wander 遭遇
- 通过对话铺垫遭遇战，允许从腰间快速抽武器到空闲的手部槽位
- 开场对白强调已穿好链甲与链甲护腿并拿上长剑
- 选择"上前交涉"时，玩家行动点设为最大值一半；抽武器则为满值
- 创建 `CombatNode` 并实例化敌人"贵族兵"：上身武装衣套链甲，腰间佩戴皮带（皮带上挂有长剑），行动点 7/10
- `Combat` 结束后释放 `CombatNode`，流程暂未推进到后续章节

## 装备管理流程
- 入口：`ShowEquipmentFlow()`，供装备菜单与测试使用
- 角色选择：无角色提示返回，单角色直接进入，多角色使用 `MenuDialogue` 选择并支持返回
- 身体部位选择：仅列出存在槽位的部位，标题使用 `BodyPart.GetNameWithEquipments()`，可随时返回上一层
- 槽位展开：
  - 展示 `VisibleInMenu` 的槽位，标题使用 `FormatSlotTitle()`（空槽 `#空`，已装备会连带子装备）
  - 描述显示允许的标志或装备描述（`FormatItemDescription()`）
  - 当容器为装备且有父槽位时，追加“卸下”选项将装备移回物品栏
- 槽位操作：
  - 空槽：筛选物品栏中标志匹配的装备生成菜单，类型不匹配会提示“无法更换”
  - 已装备：递归进入该装备的子槽位，保持嵌套容器编辑体验

## 存档与恢复
- `Save()`：写入 `Snapshot`、玩家列表、`ScriptIndex`，由装备菜单的“存档”和 `Quit()` 调用
- 读档：恢复相同结构后无缝继续章节，确保装备状态和脚本进度一致

## 协作关系
- 对话与菜单：依赖 `DialogueManager`、`GenericDialogue`、`MenuDialogue` 驱动所有交互
- 战斗：通过 `Combat` 与 `CombatNode` 建立 UI 宿主，结束后主动清理节点
- MCP：所有等待器都兼容 `GameServer.McpCheckpoint()`，远程选择可直接推进主循环

## 使用示例
```
// ProgramRoot 中创建并等待 Game
var game = new Game(saveFilePath, gameNode);
await game; // 游戏结束后继续主菜单
```

