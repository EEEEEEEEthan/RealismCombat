---
name: project-pitfalls
description: >-
  收录本仓库易错点与踩坑记录（错题集）。在用户排查 bug、做同类功能、或提到踩坑/易错/别犯同样错误时使用；先读 SKILL 中两行摘要，再按需打开 details 下对应链接全文。
---

# 项目错题集（project-pitfalls）

本 skill 为**错题索引**：每条两行——第一行 30 字以内概括错误；第二行用相对路径链接到 `details/` 中的详细说明。详情全文单独成文件，便于维护与检索。

## 错题集

`%` 子节点须勾选场景唯一名称

[详情](./details/percent-node-needs-unique-name.md)

复古像素菜单逻辑禁用勿用引擎disabled挡焦点

[详情](./details/menu-logical-disabled-needs-focus.md)

界面色除白黑透明外须从Defs取勿手写RGB

[详情](./details/ui-modulate-colors-from-defs.md)

## 维护错题集

1. **新增一条**：在上方「错题集」中追加两行——第一行错误简述（**不超过 30 个字符**）；第二行一个链接，指向本 skill 目录下 `details/` 文件夹里的独立 Markdown 文件（相对路径从 `SKILL.md` 出发，如 `./details/xxx.md`）。
2. **写详情**：在 `details/` 中新建或编辑对应 `.md`，写清背景、现象、根因、正确做法、相关文件/提交等；文件名用简短 slug（建议小写、连字符），避免中文文件名以减少跨平台问题。
3. **修改或作废**：可改详情文件；若整条不再适用，删除索引两行并酌情删除或归档详情文件。
4. **结构约定**：索引只保留两行摘要 + 链接；**不要**把长文堆在 `SKILL.md` 里，长文一律放 `details/`。
