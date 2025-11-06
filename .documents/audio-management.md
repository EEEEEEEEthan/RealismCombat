# 音频管理

`AudioManager` 作为 Godot AutoLoad 单例提供统一的音频管理：
- 位于 `Scripts/AutoLoad/` 目录
- 在 `project.godot` 中配置为自动加载单例，游戏启动时自动初始化
- 使用两个 `AudioStreamPlayer` 分别管理背景音乐（BGM）和音效（SFX）
- 提供静态方法访问，无需获取实例即可使用
- 支持播放、停止、音量控制等功能

## 使用规则

- 所有音频资源必须在 `ResourceTable` 中定义
- 直接调用静态方法 `AudioManager.PlayBgm()` 和 `AudioManager.PlaySfx()` 播放音频
- 避免在各个组件中独立创建 `AudioStreamPlayer`

