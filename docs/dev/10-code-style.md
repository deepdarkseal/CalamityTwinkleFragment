# 代码规范

> 用途：让新增代码和现有工程**长得一样**，并避开 tModLoader 1.4.4 + Calamity 的常见坑。
> 原则：先读现有实现，再写新代码；能复用就不新造；不确定的 API 先查 `ModReader/CalamityMod`。

---

## 1. 工程结构

```
Calamitytwinklefragment/
├─ Calamitytwinklefragment.cs        Mod 入口（保持极简）
├─ build.txt                          displayName / version / modReferences / loadAfter
├─ Content/
│  ├─ Items/        Weapons / Accessories / Armor / Materials / Potions / Fragment / ...
│  ├─ NPCs/         BossGod / Monster / ...
│  ├─ Projectiles/  StardustLineProj / MinusLineProj / NPCsProj / Pets / Summom
│  ├─ Buff/         Buffs / Debuffs / Pets / Summom
│  └─ TFTools/      调试与开发工具
├─ CTFplayer/       ModPlayer 集中放置
├─ System/          ModSystem（UI、冷却条等）
├─ Localization/    zh-Hans / en-US hjson
├─ Assets/          Music 等
└─ ExtraTextures/   Bestiary 等附加贴图
```

放置规则：

| 内容 | 位置 |
|---|---|
| 新武器 | `Content/Items/Weapons/<职业>/` |
| 新饰品 | `Content/Items/Accessories/`（复杂饰品可建子目录） |
| 碎片 | `Content/Items/Fragment/Fra<Name>.cs` |
| Boss | `Content/NPCs/BossGod/` |
| 小怪 | `Content/NPCs/Monster/` |
| 弹幕 | `Content/Projectiles/<系列>Proj/` |
| 减益 | `Content/Buff/Debuffs/` |
| 玩家状态 | `CTFplayer/<Name>Player.cs` |
| UI / 系统 | `System/` |

---

## 2. 命名

完整规则见 `docs/lore/05-glossary.md` §4。要点：

- 类名、文件名、贴图名、本地化键：**四者同名**。
- 物品类不加 `Item` 后缀；Boss 类不加 `Boss` 后缀。
- 新增文件、目录一律**英文**；不效仿历史遗留的中文目录。
- 命名空间镜像目录：`Calamitytwinklefragment.Content.Items.Fragment`。

---

## 3. 物品（ModItem）

标准骨架：

```csharp
public class FraExample : ModItem
{
    public override void SetStaticDefaults()
    {
        // 仅在需要时重写；空的 SetStaticDefaults 可以删掉
    }

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 22;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(0, 0, 1, 0);
        Item.rare = ItemRarityID.Blue;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        // 玩家可见文本优先走本地化键；见 11-localization.md
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Glowstick, 1)
            .AddTile(TileID.Bookcases)
            .Register();
    }
}
```

约定：

- `SetDefaults` 只放数值与默认状态，**不做世界查询、不读 `Main.rand`、不生成弹幕**。
- 复杂的 tooltip 逻辑放 `ModifyTooltips`；文本优先 hjson。
- 配方优先用 `AddIngredient<T>()` / `AddRecipeGroup`；引用 Calamity 材料时写全名并注释中文。
- `Item.rare` 用 `ItemRarityID`，不要硬编码数字。

---

## 4. Boss（ModNPC）

```csharp
[AutoloadBossHead]
public class ExampleBoss : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.MPAllowedEnemies[Type] = true; // 允许多人召唤
    }

    public override void SetDefaults()
    {
        NPC.width = 120;
        NPC.height = 120;
        NPC.damage = 100;
        NPC.defense = 50;
        NPC.lifeMax = 77777;
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.netAlways = true;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(5, 0, 0, 0);
        NPC.npcSlots = 20f;
        Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/ExampleBoss");
    }

    public override void AI()
    {
        // 状态机：enum + 计时器，见 §7
    }
}
```

约定：

- 多阶段用 `enum AttackState` + `stateTimer`，不要用魔法数字散落各处。
- 血量缩放参考现有实现（`Main.expertMode` / `Main.masterMode`），**不要**在 `SetDefaults` 里写死三份。
- 召唤物/分身多时，必须做**分组更新**或空间分区（参考 `LordOfPutrescence` 的 7 组 × 11 个 + `FluidGrid`）。
- Boss 必须处理：脱战、玩家死亡、多人同步、专家/大师掉落。
- 音乐放 `Assets/Music/<Name>`，在 `SetStaticDefaults` 或 `SetDefaults` 注册。

---

## 5. 弹幕（ModProjectile）

```csharp
public class ExampleProjectile : ModProjectile
{
    public override string Texture => "Calamitytwinklefragment/Content/Projectiles/NPCsProj/ExampleProjectile";

    public override void SetDefaults()
    {
        Projectile.width = 8;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.timeLeft = 60;
        Projectile.aiStyle = -1; // 自定义 AI 一律 -1
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0.6f);
    }
}
```

约定：

- `aiStyle = -1` 时自己实现 `AI`；不要混用原版 AI 与自定义 AI。
- 复用贴图时用 `Texture` 属性指向完整路径（现有代码有先例）。
- 弹幕数量是性能第一杀手：超过 ~200 个持续存在的弹幕，必须做对象池/分组/降频。
- 粒子用 `Dust`，但避免每帧每弹幕都生成；用计时器降频。

---

## 6. Buff / Debuff

```csharp
public class ExampleDebuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        BuffID.Sets.LongerExpertDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetModPlayer<ExamplePlayer>().exampleDebuff = true;
    }
}
```

约定：

- Debuff 的实际效果写在 `ModPlayer` 里（`ResetEffects` 重置标志位），不要在 `Update` 里直接改数值。
- `ResetEffects` 每帧重置，避免标志位残留。
- 专家延长、护士不可移除等用 `BuffID.Sets`。

---

## 7. 状态机与 AI

推荐结构：

```csharp
public enum AttackState { Idle, Chase, Cast, Recover }

public AttackState State;
public int StateTimer;

public override void AI()
{
    StateTimer++;
    switch (State)
    {
        case AttackState.Idle:
            if (StateTimer > 60) SwitchTo(AttackState.Chase);
            break;
        // ...
    }
}

private void SwitchTo(AttackState next)
{
    State = next;
    StateTimer = 0;
}
```

约定：

- 每个状态**有明确退出条件**，不允许永久停留。
- 计时器单位是帧（60 帧 = 1 秒）。
- 随机数用 `Main.rand`，但**只在服务端/AI 内使用**，不要用它决定客户端表现。
- 联机时，状态变更要同步（`ai[]` 或 `SendExtraAI`）。

---

## 8. 联机同步

| 场景 | 做法 |
|---|---|
| Boss 常驻 | `NPC.netAlways = true` |
| 自定义状态 | 用 `NPC.ai[]` 存状态与计时器（float 存 int） |
| 复杂数据 | `SendExtraAI` / `ReceiveExtraAI`，或 `ModPacket` |
| 弹幕 | 生成时 `Projectile.NewProjectile` 用 `NetMessage` 同步；不要只在客户端生成 |
| 玩家状态 | `ModPlayer` + `SyncPlayer` / `SendClientChanges` |

**检查顺序**：谁生成 → 谁拥有 → 谁会看到。三者不一致就会不同步。

---

## 9. 性能

- AI 里避免 LINQ（`Where`/`Select`/`OrderBy` 会分配）；用 `for` 循环。
- 避免每帧 `new` 对象；缓存 `List` 并复用。
- 大量同类 NPC/弹幕：分组更新（每 N 帧更新一组）+ 空间分区（网格）。
- 每帧遍历 `Main.npc` / `Main.projectile` 时，先判 `active`，再判类型。
- 复杂绘制放 `PreDraw`/`PostDraw`，并 `return false` 控制默认绘制。
- 贴图尽量共用；避免每帧 `Asset<Texture2D>` 请求。

---

## 10. Calamity API

- 引用：`CalamityMod`（`build.txt` 已声明 `modReferences` 与 `loadAfter`）。
- 查证顺序：
  1. 本地反编译：`Documents/My Games/Terraria/tModLoader/ModReader/CalamityMod/CalamityMod.dll`（可用 ILSpy/dnSpy 查看）。
  2. 官方 wiki 与源码仓库。
  3. 参考本仓库已有用法（如 `NPC.SpawnWithHigherTime`）。
- 不确定的 API **不要猜**：先用最小示例验证，或在代码里留 `// TODO(api): 待验证`。

---

## 11. 构建与提交

- 构建：tModLoader 内「构建 + 重载」，或命令行 `dotnet build`。
- **不要提交** `bin/`、`obj/`、`.vs/`；`.gitignore` 应覆盖。
- 提交前：
  - [ ] 能编译、能进游戏、能召唤/使用。
  - [ ] 本地化键无缺失（缺键会显示内部名）。
  - [ ] 贴图路径正确（缺贴图会显示粉色方块）。
  - [ ] 多人测试（至少主机 + 1 客户端）。
- 提交信息用中文，格式：`类型: 简述`（如 `设定: 新增伊尔梅里斯潮汐表碎片`）。

---

## 12. 代码注释

- 注释用中文，解释**为什么**，不复述代码。
- 复杂算法写「输入 → 处理 → 输出」三段。
- 临时方案写 `// TODO(原因): 计划`。
- 不要写 `// 设置宽度为 120` 这种废话。

```csharp
// 分组更新：77 个流体逐帧更新会拖垮 AI，改为每 7 帧轮询一组
private const int GROUPS_COUNT = 7;
```
