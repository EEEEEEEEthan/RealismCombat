# 音频管理

`AudioManager` 作为 Godot AutoLoad 单例，负责集中控制背景音乐与音效播放，避免在场景树中分散创建播放器。

## 初始化行为

- `_Ready()` 中确保单例唯一性，如检测到重复实例会立即销毁并记录错误日志
- 自动创建 `BgmPlayer` 与 `SfxPlayer` 节点并挂载到自身，默认输出到 `Master` 音频总线
- 初始化后会输出日志 `[AudioManager] 音频管理器初始化完成`，便于确认加载顺序

## 播放与控制

- `PlayBgm(AudioStream stream, bool loop = true, float volumeDb = 0f)`
  - 切换到指定曲目并播放，可按需调整音量
  - 若单例尚未初始化会打印错误并忽略请求
- `StopBgm()` 停止背景音乐播放，常用于战斗结束或场景切换
- `PlaySfx(AudioStream stream, float volumeDb = 0f)`
  - 优先复用主 `SfxPlayer`，若正在播放则动态创建额外的 `AudioStreamPlayer`
  - 额外播放器会自动绑在 `AudioManager` 作为子节点，并在播放结束后回收
  - `extraSfxBaseVolumes` 记录每个额外音效的基础音量，保证全局音量调整时可重算
- `SetBgmVolume(float volumeDb)`、`SetSfxVolume(float volumeDb)`
  - 分别控制两类音频的全局音量，`SetSfxVolume` 会同步更新所有活动播放器

## 状态查询

- `IsInitialized`：判断单例是否已经完成初始化
- `IsBgmPlaying`：BGM 播放状态
- `IsSfxPlaying`：检查主播放器与所有额外播放器，只要任意音效仍在播放即返回 `true`

## 使用规范

- 所有音频流必须在 `ResourceTable` 注册，例如 `ResourceTable.battleMusic1`
- 业务代码直接调用 `AudioManager` 静态方法，不额外保存播放器引用
- 若需要与 UI 同步音频状态，优先查询 `AudioManager` 的静态属性
- 在非游戏模式（如 MCP 自动化）也保持音频调用安全，无需额外分支

## 与其它组件的协作

- `Printer` 在 `AudioManager` 未初始化时会创建本地 `AudioStreamPlayer`，保证打字机音效可用
- 战斗动作（如 `Attack`）通过 `AudioManager.PlaySfx` 播放受击音效，与 UI 动画同步
- 可根据后续需求在 `AudioManager` 中追加淡出、音量渐变等辅助方法，所有调用者统一受益

## 背景音乐场景约定

- 主菜单与剧情流程默认播放 `ResourceTable.arpegio01Loop`，在 `ProgramRoot.StartGameLoop()` 与 `Game.StartGameLoop()` 入口即触发，避免启动静音
- 进入战斗前切换到 `ResourceTable.battleMusic1`，战斗流程结束后立即恢复剧情 BGM，保证场景切换的音乐一致性

