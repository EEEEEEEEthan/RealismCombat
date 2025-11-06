# 资源管理

项目使用资源表系统来管理和加载游戏资源：

- `ResourceTable`：管理文件资源的加载，使用延迟加载机制
- `SpriteTable`：管理从纹理图集中提取的精灵资源
- `Cache<T>` 和 `Loader<T>`：提供延迟加载和缓存功能

## 重要规则

- 所有游戏资源（纹理、音频、场景等）必须通过 `ResourceTable` 或 `SpriteTable` 加载
- 禁止在代码中直接使用 `GD.Load()` 加载资源
- 新增资源时，先在 `ResourceTable` 中添加对应的 `Loader` 定义
- 这样可以确保资源的统一管理和延迟加载

