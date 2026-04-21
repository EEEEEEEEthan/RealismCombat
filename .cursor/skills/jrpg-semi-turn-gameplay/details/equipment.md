# 装备玩法

## 数据模型（与代码对应）

- **角色** `Character`：`Head`（当前无装备槽）、`Body`、`Hand`×2、`Foot`×2；聚合在 `all_body_parts`。
- **槽位** `ItemSlot`：持有 `item`，构造时传入**允许的脚本类型数组**。赋值时 `allows_item` 沿物品脚本的**继承链**（`get_script()` → `get_base_script()`）与白名单比对，不匹配则拒绝并 `push_error`。
- **物品** `Item`：抽象基类；`quality` / `protection`（按品质倍率从 `_base_protection` 派生）；可覆盖 `get_item_slots()` 暴露**子槽**，形成装备树上的嵌套。

## 身体部位与槽位

| 部位 | 槽位 | 允许类型 |
|------|------|----------|
| 身体 `Body` | `torso_slot`（单槽，叠穿靠嵌套） | `InnerLiningGarment`、`OuterwearCoat`、`Belt` |
| 手 `Hand` | `glove_slot` | `Glove`（须与左右手 `Defs.Side` 一致，见下） |
| 手 `Hand` | `weapon_slot` | `Weapon` |
| 脚 `Foot` | `footwear_slot` | `Footwear`（须与左右脚一致） |

手套、鞋靴、手持武器槽是**同一部位上的并列槽**，彼此不嵌套。

## 躯干叠穿（嵌套规则）

躯干只有**一个根槽** `body.torso_slot`，其上放「贴身体」的那一件；更外层通过该物品（或链上每一件）的 `over_slot` 或腰带的子槽继续挂接。类注释中的语义：**中层/外层护甲叠穿须从内衬起**，在工程里体现为**允许的脚本类型组合**，而不是多个平行躯干槽。

自内向外的链路由各类的 `over_slot` 白名单定义：

1. **`InnerLiningGarment`（内衬）**  
   `over_slot` 可装：`MiddleLayerArmor`、`OuterwearCoat`、`Belt`  
   例：`GambesonInner`。

2. **`MiddleLayerArmor`（中层护甲）**  
   `over_slot` 可装：`OuterLayerArmor`、`OuterwearCoat`、`Belt`  
   例：`ChainMailHauberk`。

3. **`OuterLayerArmor`（外层护甲）**  
   `over_slot` 可装：`OuterwearCoat`、`Belt`  
   例：`PlateArmorSuit`。

4. **`OuterwearCoat`（外套）**  
   `over_slot` **仅**可装：`Belt`  
   例：`SurcoatTabard`（罩袍）。

5. **`Belt`（腰带，抽象）**  
   无「再往外穿」的躯干叠槽；构造时生成若干 **`weapon_slots`**，每格仅 `Weapon`。  
   例：`LeatherBelt` 为 4 格武器槽。

因此合法的一条「由内向外的叠穿链」在数据上是一条**单向嵌套**：例如 内衬 → 锁甲 → 板甲 → 罩袍 → 皮带；也可在某一环截断（如身体槽直接挂外套或腰带，由 `torso_slot` 的允许类型支持）。**外套之上只能再接腰带**，不能再挂中层/外层甲。

## 装备 UI 行为（`EquipmentMenuFlow`）

- 导航：角色 → 身体部位 → 该部位 `get_item_slots()` 列表 → 若槽内有物，进入该物品的子槽菜单（递归）；空槽则从背包列出 `slot.matching_inventory_items` 的候选。
- **左右匹配**：从脚部位装 `Footwear`、从手部位装 `Glove` 时，比较 `BodyPart.side` 与物品的 `side`，不匹配则在菜单里标记为不可用（仍显示）。
- **卸下**：在宿主物品的子槽菜单中选「卸下」，会清空**父槽**（整件卸下回背包），而非只卸嵌套子件。

## 与战斗/属性

- 物品提供防护等（`Item.protection`）；品质变化会失效缓存并更新防护。
- 手持 `weapon_slot` 与腰带上的武器槽为数据结构的一部分；当前默认角色动作仍以徒手等为主，战斗逻辑是否读取哪一把武器需以实际动作/校验代码为准。

## 维护约定

新增装备类型时：明确其脚本继承哪一类（以接入正确的 `ItemSlot` 白名单）；若参与躯干叠穿，在对应父类的 `over_slot` 白名单中声明允许的子类型；需要子槽时覆盖 `get_item_slots()`。
