# 常见坑与历史遗留

> 用途：先查这里，再 debug。记录的是**本仓库 + tModLoader 1.4.4 + Calamity** 环境下的真实问题。

---

## 1. 环境与构建

### 1.1 环境基线

| 项 | 值 |
|---|---|
| tModLoader | 1.4.4，.NET 8（`tModLoader.runtimeconfig.json` 为 `net8.0`） |
| 游戏目录 | `D:\steam\steamapps\common\tModLoader` |
| 数据目录 | `%USERPROFILE%\Documents\My Games\Terraria\tModLoader` |
| 源码 | `...\tModLoader\ModSources\Calamitytwinklefragment` |
| Calamity 引用 | `..\..\ModReader\CalamityMod\CalamityMod.dll`（由 tML 在启动时解包生成） |

### 1.2 构建设置

- 优先用 tModLoader 内「构建 + 重载」；命令行 `dotnet build` 需要正确的 SDK 与引用路径。
- `ModReader\CalamityMod\CalamityMod.dll` **不是永久文件**：Calamity 更新或重新解包后路径不变但内容会变。构建前确认 Calamity 已加载。
- 若报「找不到 CalamityMod」，先启动一次 tML 让 ModReader 重新生成。

### 1.3 首次提交

本仓库 `git` 已初始化但**尚无任何提交**（`master` 分支，所有文件 untracked）。首次提交前确认 `.gitignore` 覆盖：

```
bin/
obj/
.vs/
*.user
```

不要把 `bin/`、`obj/`、`.vs/` 提交进去。

---

## 2. 本地化

| 现象 | 原因 | 处理 |
|---|---|---|
| 物品名显示为 `FraExample` | 缺 `Items.FraExample.DisplayName` | 补键；构建后重载 |
| 显示 `Mods.Calamitytwinklefragment...` | 键路径错误或类名改动 | 键名必须与类名完全一致 |
| 中文正常，英文空白 | 只补了 `zh-Hans` | 同步 `en-US` |
| Tooltip 整段消失 | hjson 语法错误导致整文件失效 | 检查 Tab/引号/三引号 |
| Shift 文本不显示 | 自定义键不会被自动生成 | 手动加 `Whisper1..N` |
| 文本乱码 | 存成了 GBK | 另存为 UTF-8（无 BOM） |
| 改完没生效 | 只保存未构建/未重载 | 构建 + 重载；必要时重启 |

**注意**：自动生成的条目只在**构建后**出现；手动键（`WhisperN`、`MoonlightTime` 等）永远要手写。

---

## 3. 贴图

| 现象 | 原因 | 处理 |
|---|---|---|
| 粉色方块 | 贴图缺失或路径错 | 贴图与类名同名、同目录 |
| 翅膀/宠物显示异常 | 缺少 `_Wings`、`_Head` 等后缀贴图 | 按原版命名规则补 |
| Boss 贴图过大/错位 | `NPC.frame` 与 `npcFrameCount` 不匹配 | 单帧就设 `Main.npcFrameCount[Type] = 1` |
| 弹幕贴图复用失败 | `Texture` 路径不是完整命名空间路径 | 用 `"Calamitytwinklefragment/Content/..."` 全路径 |

---

## 4. 中文目录与文件名

现有遗留：

- `Content/Projectiles/spine表/`
- `Content/Buff/Debuffs/硫磺海剧毒.png`

Windows 下能编译，但：

- Git 跨平台、CI、部分工具链可能出问题；
- 大小写/编码不一致会导致「本地能跑、别人拉下来不能跑」。

**规则**：旧的不动，**新增一律英文**。

---

## 5. 联机同步

| 现象 | 原因 | 处理 |
|---|---|---|
| 客机看不到 Boss 弹幕 | 弹幕只在客户端生成 | 用 `Projectile.NewProjectile` 在服务端/拥有者生成，或 `NetMessage` 同步 |
| Boss 状态不同步 | 只改了本地字段 | 状态与计时器放 `NPC.ai[]`，或 `SendExtraAI` |
| 客机看不到小怪 | `netAlways` 缺失 | Boss 设 `NPC.netAlways = true` |
| 血量不一致 | 各自算各自的 | 血量由服务端权威；客户端只读 |
| 玩家死亡后 Boss 不脱战 | 未处理目标失效 | `NPC.TargetClosest` + `EncourageDespawn` |

**检查顺序**：谁生成 → 谁拥有 → 谁会看到。

---

## 6. Boss 常见问题

1. **不脱战**：玩家死亡/离开后必须 `TargetClosest(false)`，仍无目标则 `EncourageDespawn`。
2. **没有音乐**：`Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/X")`，文件必须真实存在。
3. **没有血条**：`NPC.boss = true`；自定义血条需额外处理。
4. **多人血量不涨**：按 `Main.expertMode` / `Main.masterMode` 或 Calamity 难度缩放，不要写死。
5. **召唤重复**：召唤物 `CanUseItem` 里判 `NPC.AnyNPCs(...)`。
6. **Boss 卡墙**：`noTileCollide = true` 或写贴墙逻辑；不要两者都不做。
7. **二阶段不触发**：确认血量阈值用 `NPC.life` 而非 `NPC.lifeMax`，且只触发一次（布尔标志）。

---

## 7. 物品与配方

| 现象 | 原因 | 处理 |
|---|---|---|
| 配方不显示 | 材料类型写错/未注册 | 用 `ModContent.ItemType<T>()`，不要 `new` |
| 物品无法堆叠 | `maxStack` 默认 1 | 按需设 `Item.maxStack` |
| 召唤物被消耗 | `consumable` 默认值 | 明确设 `true/false` |
| Calamity 材料找不到 | 命名空间/类名变更 | 查 `ModReader` 反编译，或参考现有 `using` |
| 合成站无效 | `AddTile` 用了错误 ID | 用 `TileID.*` 或 `ModContent.TileType<T>()` |

---

## 8. Buff / Debuff

| 现象 | 原因 | 处理 |
|---|---|---|
| 减益效果残留 | 未在 `ResetEffects` 重置 | 每帧重置标志位 |
| 护士能移除 | 未设 `NurseCannotRemoveDebuff` | 补 `BuffID.Sets` |
| 专家时长不变 | 未设 `LongerExpertDebuff` | 补 `BuffID.Sets` |
| 减益图标不显示 | 缺同名 png | 补 `Content/Buff/Debuffs/<Name>.png` |
| 减益不造成伤害 | 只写了 `Update` | 在 `ModPlayer.UpdateBadLifeRegen` 里处理 |

---

## 9. 性能

- **大量小怪**：参考 `LordOfPutrescence` 的 7 组 × 11 个分组更新 + `FluidGrid` 空间分区。
- **大量弹幕**：降频生成 Dust、限制同屏数量、复用贴图。
- **AI 里不要 LINQ**：每帧分配会触发 GC 峰值。
- **不要每帧遍历全部 NPC/Projectile**：先 `active` 再类型判断。
- **复杂绘制**：`PreDraw` 返回 `false` 接管，避免默认绘制叠加。

---

## 10. 版本与兼容

- Calamity 已停止开发（官方公告），但**本地版本仍可能与你安装的 tML 版本不匹配**。
- Calamity 更新可能重命名 API；升级后先构建一次，看编译错误再改。
- 本项目 `build.txt` 声明 `loadAfter = CalamityMod`，不要删。
- 不要在代码里硬编码 Calamity 的内部常量；用其公开 API 或 `ModContent`。

---

## 11. 历史遗留清单

| 位置 | 问题 | 建议 | 优先级 |
|---|---|---|---|
| `FraMoldyPaper.cs`、`FraArmsOrder.cs`、`FragmentTravelAdvertisement.cs`、`FraBloodGodBattleRoar.cs` | 中文 tooltip 硬编码在代码里 | 迁移到 hjson（见 `11-localization.md` §9） | 中 |
| `GodnessArrowhed` | 拼写错误（应为 `GoddessArrowhead`） | 重构时统一改名 | 低 |
| `Content/Projectiles/spine表/` | 中文目录 | 保留；新内容勿效仿 | 低 |
| `Localization` 空键（`WeaverMinionBuff` 等） | 未翻译 | 逐步补 | 低 |
| `description.txt` | 仍是模板文案 | 写正式简介 | 中 |
| `Content/Items/.../AbyssalCrushWave.cs.cs` | 双重扩展名 | 建议改为 `.cs` | 低 |

---

## 12. 快速诊断

遇到问题按顺序排查：

1. **能编译吗？** 不能 → 看第一条编译错误，通常是命名空间/类型/引用。
2. **进游戏了吗？** 不能 → 看 `Logs` 目录最新日志。
3. **物品/ Boss 出现了吗？** 没有 → 类是否 `public`、是否在正确命名空间、是否被 autoload。
4. **名字/文本对吗？** 显示内部名 → 本地化键。
5. **贴图对吗？** 粉色方块 → 贴图路径/命名。
6. **多人对吗？** 单机正常联机异常 → 同步。
7. **卡吗？** 掉帧 → 数量、LINQ、Dust、绘制。

> 日志位置：`Documents/My Games/Terraria/tModLoader/Logs/`（按时间取最新）。

---

## 13. 不要做的事

- 不要为了让代码「跑起来」而改设定文本。
- 不要在 `SetDefaults` 里做重逻辑或读世界状态。
- 不要在客户端决定 Boss 血量/掉落。
- 不要提交 `bin/`、`obj/`、`.vs/`、`*.user`。
- 不要一次提交里混入「重命名 + 改文案 + 重构」三类改动。
