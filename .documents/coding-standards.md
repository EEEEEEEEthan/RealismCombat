# 开发规范

## 架构约定

- 组件初始化时应通过构造函数注入必需依赖，避免额外的初始化 setter，降低遗漏调用的风险
- 项目启用了 C# 可空注解分析，代码应信任类型签名，正常逻辑下不编写额外的空值防御
- 初始化流程能在单个构造函数或方法里一次性完成时，应避免拆分成多步调用，保持操作的原子性
- Godot AutoLoad 单例负责跨场景服务（如 `DialogueManager`、`AudioManager`、`GameServer`），普通节点不直接缓存静态引用

## 节点命名规范

- 在代码中动态创建的节点必须设置语义化名字，方便调试与路径访问
- 自定义节点建议在 `_EnterTree()` 设置 `Name`，避免遗漏构造阶段初始化
- 内置节点可在对象初始化器或 `AddChild()` 前设置 `Name`，不要使用 Godot 默认的 `@ClassName@ID`

```csharp
public override void _EnterTree()
{
	base._EnterTree();
	Name = "Dialogue";
}

var player = new AudioStreamPlayer
{
	Name = "BgmPlayer",
	Bus = "Master",
};
```

## 目录组织与资源加载

- 所有场景放在 `Scenes/`，脚本放在 `Scripts/`，保持描述性文件名
- 纹理、音频等二进制资源对应放在 `Textures/`、`Audios/`，并通过 `.import` 文件维护
- 任何资源访问都应通过 `ResourceTable` 或 `SpriteTable`，禁止直接调用 `GD.Load`
- 复用 `Cache<T>` 与 `Loader<T>` 提供的延迟加载能力，避免在 `_Ready()` 等生命周期重复 IO

## 异步流程与等待器

- 需要被 `await` 的流程使用 `TaskCompletionSource` 封装等待器，并通过 `GetAwaiter()` 暴露
- `async void` 仅用于 Godot 生命周期入口（例如 `_Ready()`、`StartGameLoop()`），其它异步方法返回 `Task`
- `GameServer.McpCheckpoint()` 会唤醒 MCP 等待，应在完成关键步骤后调用，避免客户端挂起
- 操作节点前先通过 `this.Valid()` 检查实例有效性，防止节点已被释放导致的异常

## 日志与异常处理

- 使用 `Log.Print`、`Log.PrintWarning`、`Log.PrintError` 输出信息，禁止直接调用 `GD.Print`
- 捕获异常时调用 `Log.PrintException`，保持异常细节完整输出
- MCP 指令处理、异步循环等关键路径在捕获异常后必须调用 `GameServer.McpCheckpoint()`，保证远端同步
- 临时调试输出需要在提交前清理，避免噪声影响日志收集

## 二进制序列化与兼容性

- 所有序列化结构使用 `ReadScope()`、`WriteScope()` 维护块长度，确保读取失败时可以定位错误
- 添加字段时保持旧字段顺序不变，如需破坏性变更需提升 `GameVersion`
- 字符串序列化使用 `BinaryWriter.Write(string)`，长度由框架处理，不额外写终止符
- 使用 `using (reader.ReadScope())` 与 `using (writer.WriteScope())` 包裹结构体读写，避免数据错位

## 代码风格补充

- 能通过属性表达的场景尽量使用属性，保持 API 简洁一致
- `IDisposable` 对象释放优先调用 `TryDispose()` 扩展方法，减少异常传播
- 事件触发使用 `TryInvoke()`，保证订阅者异常不会破坏主流程
- UI 组件在构造阶段搭建完整节点树，后续逻辑仅修改状态，避免运行时频繁增删节点
