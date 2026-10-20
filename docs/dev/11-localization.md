# 本地化规范

> 用途：让每一句玩家可见的话都进 hjson，中英同步，键名可预测。
> 目标：**代码里看不到中文台词**；换语言时不会出现内部类名或空白。

---

## 1. 文件与编码

| 文件 | 语言 |
|---|---|
| `Localization/zh-Hans_Mods.Calamitytwinklefragment.hjson` | 简体中文 |
| `Localization/en-US_Mods.Calamitytwinklefragment.hjson` | 英文 |

- 编码：**UTF-8（无 BOM）**。
- 缩进：**Tab**（与现有文件一致）。
- 换行：CRLF 或 LF 均可，但同一文件保持一致。
- 文件头保留自动生成注释，不要删。

---

## 2. 键结构

```hjson
Items: {
    <类名>: {
        DisplayName: <显示名>
        Tooltip:
            '''
            第一行
            第二行
            '''
    }
}
```

| 内容类型 | 键 |
|---|---|
| 物品 | `Items.<Class>.DisplayName` / `Items.<Class>.Tooltip` |
| NPC | `NPCs.<Class>.DisplayName` |
| Buff | `Buffs.<Class>.DisplayName` / `Buffs.<Class>.Description` |
| 弹幕 | `Projectiles.<Class>.DisplayName` |
| 自定义 TooltipLine | `Items.<Class>.<自定义键>`（推荐）或统一放 `Items.<Class>.Tooltip` 多行 |

> 当同一类名在多个类别下重复（如 `WovenWings` 既是物品又是饰品），用**完整路径**分组：
> `Calamitytwinklefragment/Content/Items/Accessories/Mana/StarFlameStone: { ... }`

---

## 3. 多行文本

hjson 三引号块：

```hjson
Tooltip:
    '''
    连续劈砍会强化刀刃
    击中敌人引发爆炸
    [c/ff4048:“它被染上深红 枯了又荣”]
    '''
```

注意：

- `'''` 必须独占一行。
- 块内**不要**再写 Tab 缩进以外的格式；行首空格会被保留。
- 行内引号用中文引号「」或 `“”`，避免与 hjson 语法冲突。
- 不要用 `\n`，直接换行。

---

## 4. 颜色与格式标记

| 标记 | 用途 | 示例 |
|---|---|---|
| `[c/HEX:文本]` | 染色 | `[c/ff4048:深红]` |
| `[i:物品ID]` | 物品图标 | 少用，碎片文本不用 |
| `{0}` | 占位符 | `月光附魔剩余：{0}秒` |

规则：

- 颜色码统一小写十六进制，6 位。
- 一句里不要超过 2 种颜色。
- 碎片原文的颜色由**代码**的 `OverrideColor` 控制，hjson 里不要再套 `[c/...]`（二选一，避免叠加）。
- 专属物品标记沿用现有格式：`[c/5e8e4d:- 专属物品 -]`。

---

## 5. 碎片（吉光片羽）的固定格式

中文：

```hjson
FraExample: {
    DisplayName: 吉光片羽：示例物
    Tooltip: 寄宿其上的低语流入你的耳中……
}
```

英文：

```hjson
FraExample: {
    DisplayName: Twinkle Fragment: Example
    Tooltip: Whispers bound to it flow into your ears...
}
```

Shift 展开的原文：

- **方案 A（推荐）**：放 hjson 自定义键，代码只负责按 Shift 追加。

```hjson
FraExample: {
    DisplayName: 吉光片羽：示例物
    Tooltip: 寄宿其上的低语流入你的耳中……
    Whisper1: 第三铸造区医务所 通告第 44 号
    Whisper2: 即日起，烧伤止疼剂停发。
    Whisper3: 伤重不能上工者，按「非工伤」处理。
}
```

```csharp
// 代码里只取键，不写文案
tooltips.Add(new TooltipLine(Mod, "Whisper1", Language.GetTextValue($"Mods.{Mod.Name}.Items.{Name}.Whisper1")));
```

- **方案 B（兼容旧代码）**：沿用 `LegendaryItemExtra40x` 硬编码，但**新内容一律用方案 A**。

---

## 6. 命名与文案规则

1. 键名 = 类名，不改写、不翻译、不加前缀。
2. 显示名用官方译名（查 `05-glossary.md`），不自创。
3. 碎片显示名固定格式：`吉光片羽：<物名>`；英文 `Twinkle Fragment: <Name>`。
4. Tooltip 里不写数值以外的设定解释；数值用阿拉伯数字 + `%`。
5. 英文文案不要机翻：短句、主动语态、专名首字母大写。
6. 同一物品的中英条目**必须同时存在**；缺英文时先写 `TODO(i18n)` 并记录，不能留空。

---

## 7. 新增内容的本地化流程

1. 建类 `FraExample`。
2. 构建一次（tModLoader 会自动把新键写入两个 hjson）。
3. 打开 `zh-Hans` 填中文；打开 `en-US` 填英文。
4. 自定义键（如 `Whisper1`）需**手动添加**，自动生成不会包含。
5. 重载，进游戏检查：
   - 物品名不是 `FraExample`；
   - Tooltip 不是空；
   - Shift 展开正常；
   - 英文语言下不出现中文。

---

## 8. 常见错误

| 现象 | 原因 | 处理 |
|---|---|---|
| 显示 `Mods.Calamitytwinklefragment.Items.FraExample.DisplayName` | 键缺失或拼错 | 检查类名与键名 |
| 显示类名 `FraExample` | 同上 | 补键 |
| 中文正常、英文空白 | 只补了一边 | 补 `en-US` |
| hjson 解析失败、整个文件失效 | 少了逗号/多了逗号、引号未闭合、Tab 混空格 | 用 hjson 校验；保持 Tab |
| Shift 文本不显示 | `ModifyTooltips` 未追加，或键不存在 | 检查代码与键 |
| 颜色不生效 | `[c/...]` 写成了 `[C/...]` 或颜色值非 6 位 | 统一小写 6 位 |
| 中文乱码 | 文件存成了 GBK | 另存为 UTF-8 |

---

## 9. 批量迁移建议

历史代码里有硬编码中文（如 `FraMoldyPaper`、`FraArmsOrder`）。迁移时：

1. **一次只迁移一个物品**，不要顺手改文案。
2. 先加 hjson 键，值与原文**逐字一致**。
3. 再把代码改为读键。
4. 构建、进游戏对比，确认一字不差。
5. 单独提交，提交信息写 `本地化: 迁移 FraMoldyPaper 文本`。

---

## 10. 英文文案速查

| 中文 | 英文 |
|---|---|
| 寄宿其上的低语流入你的耳中…… | Whispers bound to it flow into your ears... |
| 按住「左Shift」仔细聆听 | Hold [Left Shift] to listen closer |
| 吉光片羽：发霉的论文 | Twinkle Fragment: Moldy Paper |
| 吉光片羽：武器订单 | Twinkle Fragment: Arms Order |
| 吉光片羽：旅游广告 | Twinkle Fragment: Travel Advertisement |
| 吉光片羽：血神战吼 | Twinkle Fragment: Blood God's Battle Roar |
| 月女神 露娜蒂卡 | Lunatica, the Moon Goddess |
| 腐败之主 | Lord of Putrescence |
| 永恒腐败 | Eternal Decay |
| 锁定 | Luna Locked |
| 癫狂银月 | Crazy Silver Moon |
| 腐败坩埚 | Putrid Cauldron |
