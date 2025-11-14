# RealismCombat 项目文档

这是一个基于 Godot 的文字游戏项目，使用 C# 开发。

## 文档目录

### 开发规范
- [开发规范](coding-standards.md) - 节点命名规范等开发约定

### 核心系统
- [战斗系统](combat-system.md) - 战斗流程、输入决策与角色属性
- [角色系统](character-system.md) - Character、BodyPart 身体部位与装备挂载
- [物品系统](item-system.md) - Item、ItemSlot、装备类型与序列化
- [属性系统](property-system.md) - PropertyInt、PropertyDouble 数值属性
- [资源管理](resource-management.md) - ResourceTable、延迟加载机制
- [音频管理](audio-management.md) - AudioManager 单例系统
- [UI 系统](ui-system.md) - DialogueManager、对话框、菜单、异步等待机制
- [扩展方法](extensions.md) - 实用的扩展方法

### 运行时流程
- [运行时流程](runtime-flow.md) - ProgramRoot、Game、MCP 模式与场景结构

### 工具与服务
- [日志系统](log-system.md) - Log、LogListener 日志输出与收集
- [版本系统](game-version.md) - GameVersion 版本号格式与比较
- [游戏颜色](game-colors.md) - GameColors 颜色常量与梯度
- [调试指南](debugging.md) - 调试工具和常见问题解决方法
- [MCP 服务器](mcp-server.md) - 用于测试和调试的 MCP 服务器
