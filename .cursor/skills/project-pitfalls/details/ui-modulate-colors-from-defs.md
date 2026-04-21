# 界面 modulate 等色调须走 Defs

## 背景

项目用 `Defs.get_family_color` 等统一色阶，避免各处魔法数字不一致。

## 现象

为禁用态、强调态等直接写 `Color(0.55, 0.55, 0.55)` 或随意 RGB，换主题或色阶时要全库搜字面量，且与全局调色脱节。

## 根因

未把「非例外色」集中到 `Defs`（或已有 helper），而是就地硬编码。

## 正确做法

- **除约定例外**（如 `Color.WHITE`、`Color.BLACK`、`Color.TRANSPARENT` 等字面量）外，菜单/ HUD 的 modulate、主题色等应 **`Defs.get_family_color` 或专用静态方法**（如 `Defs.get_menu_option_disabled_modulate()`），**禁止**手写灰度常量凑效果。
- 与「禁用但仍可聚焦」的交互拆分：焦点与 `pressed` 拦截见 [menu-logical-disabled-needs-focus.md](./menu-logical-disabled-needs-focus.md)。

## 相关文件

- `defs.gd`
- `dialogues/menu_dialogue.gd`
- `dialogues/generic_dialogue.gd`
