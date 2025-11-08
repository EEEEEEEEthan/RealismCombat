# 开发规范

## 架构约定

- 组件初始化时应通过构造函数注入必需依赖，避免额外的初始化 setter，降低遗漏调用的风险。

## 节点命名规范

**所有在代码中动态创建的节点都必须赋予有意义的名字：**

- 使用 `Name` 属性为节点设置清晰、描述性的名字
- 对于自定义节点类，在 `_EnterTree()` 方法中设置 `Name`
- 对于内置节点类（如 `AudioStreamPlayer`），可在对象初始化器中设置或在父节点中设置
- 避免使用默认生成的名字（如 `@ClassName@ID`）

**原因：**
- 便于通过场景树调试和定位问题
- 提高代码可维护性和可读性
- 方便使用节点路径获取节点

**示例：**
```csharp
// 自定义节点类
public override void _EnterTree()
{
    base._EnterTree();
    Name = "Dialogue";
}

// 内置节点类
var player = new AudioStreamPlayer()
{
    Name = "BgmPlayer",
    Bus = "Master",
};
```

