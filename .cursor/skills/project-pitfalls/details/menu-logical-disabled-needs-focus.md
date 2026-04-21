# 复古像素菜单：逻辑禁用须保留焦点

## 背景

本作为**复古像素**风格，列表菜单大量依赖方向键/手柄在选项间移动焦点。在此 UX 下，**「不可用」项通常仍要出现在焦点链里**（看得见、能选中、读出说明），只是**不能确认**。

## 现象

把「不可用」直接映射为 `BaseButton.disabled = true` 时，该项会**无法被聚焦**，导航会跳过，与上述常态不符。

## 根因

Godot 的 `BaseButton.disabled` 会按引擎语义关掉可聚焦等交互。**本项目的「禁用」多数是业务逻辑上的不可用**，不是「控件失效」，因此不能单靠引擎 `disabled` 表达「灰显 + 可选中但不确认」。

## 正确做法

- 用数据（如 `MenuItemData.disabled`）表示逻辑禁用；按钮保持 `disabled = false`，保证**仍可聚焦**。
- 在 `pressed` 回调里若逻辑禁用则**不**向外 `emit`。
- 视觉上的「不可用」用 modulate 等表达（色调约定见 [ui-modulate-colors-from-defs.md](./ui-modulate-colors-from-defs.md)）。

## 相关文件

- `dialogues/menu_dialogue.gd`
- `dialogues/generic_dialogue.gd`
