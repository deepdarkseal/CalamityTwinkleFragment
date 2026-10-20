# 内容模板

> 用途：复制 → 改名 → 填内容。模板只保证**结构正确**，不保证数值平衡。
> 所有模板都以「能编译、能进游戏」为最低标准；标 `TODO` 的地方必须处理。

---

## 1. 碎片物品模板

对应文档：`docs/lore/04-fragments.md`、`docs/lore/03-events.md`。

```csharp
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Fragment
{
    /// <summary>
    /// 吉光片羽：示例物。
    /// 设定：AC 1870，阿萨福勒第三铸造区，医务所通告。
    /// 依据：docs/lore/03-events.md E-02；docs/lore/04-fragments.md 范例一。
    /// </summary>
    public class FraExample : ModItem
    {
        // 与 hjson 中 Whisper1..WhisperN 的条数保持一致
        private const int WhisperCount = 6;

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(0, 0, 1, 0);
            Item.rare = ItemRarityID.Blue; // 类型对应的稀有度见 04-fragments.md §7
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 未按 Shift 时只显示基础 Tooltip
            if (!Main.keyState.IsKeyDown(Keys.LeftShift))
                return;

            for (int i = 1; i <= WhisperCount; i++)
            {
                string key = $"Mods.{Mod.Name}.Items.{Name}.Whisper{i}";
                string text = Language.GetTextValue(key);

                // 键不存在时 Language 会回退为键名，跳过以免暴露内部键
                if (text == key)
                    continue;

                tooltips.Add(new TooltipLine(Mod, $"Whisper{i}", text)
                {
                    // 颜色随碎片类型变化，见 04-fragments.md §7
                    OverrideColor = new Color(139, 26, 26)
                });
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Book, 1)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}
```

配套本地化（两边都要）：

```hjson
FraExample: {
    DisplayName: 吉光片羽：示例物
    Tooltip: 寄宿其上的低语流入你的耳中……
    Whisper1: 第三铸造区医务所 通告第 44 号
    Whisper2: 即日起，烧伤止疼剂停发。
    Whisper3: 硫磺粉尘吸入者，改领湿布与盐。
    Whisper4: 伤重不能上工者，按「非工伤」处理。
    Whisper5: 望各车间体谅时艰。
    Whisper6: ——落款处被火燎去，只剩半个印。
}
```

```hjson
FraExample: {
    DisplayName: Twinkle Fragment: Notice of Suspension
    Tooltip: Whispers bound to it flow into your ears...
    Whisper1: Third Foundry District Infirmary, Notice No. 44
    Whisper2: Effective immediately, burn analgesics are discontinued.
    Whisper3: Those with sulphur-dust inhalation will receive wet cloth and salt instead.
    Whisper4: Injuries preventing work are classified as "not work-related".
    Whisper5: We ask each workshop to understand the hardship.
    Whisper6: — The signature is scorched away; only half a seal remains.
}
```

---

## 2. Boss 骨架

```csharp
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.NPCs.BossGod
{
    /// <summary>
    /// 示例 Boss。数值、阶段与掉落均为占位。
    /// </summary>
    [AutoloadBossHead]
    public class ExampleBoss : ModNPC
    {
        public enum AttackState
        {
            Idle,
            Chase,
            Cast,
            Recover
        }

        public AttackState State;
        public int StateTimer;
        public bool SecondPhase;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.MPAllowedEnemies[Type] = true; // 允许多人模式召唤
        }

        public override void SetDefaults()
        {
            NPC.width = 120;
            NPC.height = 120;
            NPC.damage = 100;
            NPC.defense = 50;
            NPC.lifeMax = 77777;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.netAlways = true;
            NPC.value = Item.buyPrice(5, 0, 0, 0);
            NPC.npcSlots = 20f;

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/ExampleBoss"); // TODO: 放入 mp3/ogg
        }

        public override void AI()
        {
            StateTimer++;

            // 阶段切换：血量低于 50% 进入二阶段（仅执行一次）
            if (!SecondPhase && NPC.life < NPC.lifeMax * 0.5f)
            {
                SecondPhase = true;
                SwitchTo(AttackState.Cast);
            }

            switch (State)
            {
                case AttackState.Idle:
                    if (StateTimer > 60)
                        SwitchTo(AttackState.Chase);
                    break;

                case AttackState.Chase:
                    ChaseTarget();
                    if (StateTimer > 180)
                        SwitchTo(AttackState.Cast);
                    break;

                case AttackState.Cast:
                    CastAttack();
                    if (StateTimer > 120)
                        SwitchTo(Recover);
                    break;

                case AttackState.Recover:
                    if (StateTimer > 45)
                        SwitchTo(AttackState.Chase);
                    break;
            }
        }

        private void SwitchTo(AttackState next)
        {
            State = next;
            StateTimer = 0;
        }

        private void ChaseTarget()
        {
            Player target = Main.player[NPC.target];

            // 目标失效时脱战，避免 Boss 卡在空场
            if (!target.active || target.dead)
            {
                NPC.TargetClosest(false);
                target = Main.player[NPC.target];
                if (!target.active || target.dead)
                {
                    NPC.EncourageDespawn(10);
                    return;
                }
            }

            Vector2 desired = target.Center - NPC.Center;
            desired.Normalize();
            desired *= 6f;
            NPC.velocity = (NPC.velocity * 20f + desired) / 21f;
        }

        private void CastAttack()
        {
            // 示例：每 30 帧发射一次弹幕
            if (StateTimer % 30 != 0)
                return;

            Vector2 direction = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 8f;
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction, ProjectileID.Fireball, 30, 3f);
        }

        public override void OnKill()
        {
            // TODO: 首杀掉落、Boss 袋、专属碎片
        }
    }
}
```

> `BossLoot` 等签名在不同 tML 版本间有差异，**先查 `ModReader` 或官方文档再用**；不确定就用 `NPCLoot()` + `OnKill()`。

---

## 3. Boss 召唤物模板

```csharp
public class ExampleSummon : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.consumable = false;   // 按设定决定是否消耗
        Item.rare = ItemRarityID.Purple;
        Item.value = Item.buyPrice(0, 10, 0, 0);
    }

    public override bool CanUseItem(Player player)
    {
        // 夜晚才能召唤（示例）
        if (Main.dayTime)
            return false;

        return !NPC.AnyNPCs(ModContent.NPCType<ExampleBoss>());
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<ExampleBoss>());
            SoundEngine.PlaySound(SoundID.Roar, player.position);
        }
        return true;
    }
}
```

---

## 4. 弹幕模板

```csharp
public class ExampleProjectile : ModProjectile
{
    public override string Texture => "Calamitytwinklefragment/Content/Projectiles/NPCsProj/ExampleProjectile";

    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.timeLeft = 120;
        Projectile.penetrate = 1;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        Projectile.rotation += 0.2f;

        // 粒子降频：每 3 帧一次，避免大量弹幕拖帧
        if (Projectile.timeLeft % 3 == 0)
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Silver);

        Lighting.AddLight(Projectile.Center, 0.3f, 0.3f, 0.5f);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(BuffID.OnFire, 180);
    }
}
```

---

## 5. 减益 + ModPlayer 模板

减益本身只负责「标记」，实际效果放 `ModPlayer`：

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
        player.GetModPlayer<ExamplePlayer>().ExampleDebuff = true;
    }
}
```

```csharp
public class ExamplePlayer : ModPlayer
{
    public bool ExampleDebuff;

    public override void ResetEffects()
    {
        ExampleDebuff = false; // 每帧重置，避免残留
    }

    public override void UpdateBadLifeRegen()
    {
        if (!ExampleDebuff)
            return;

        if (Player.lifeRegen > 0)
            Player.lifeRegen = 0;

        Player.lifeRegenTime = 0;
        Player.lifeRegen -= 8; // 每秒约 4 点
    }
}
```

---

## 6. 本地化条目模板

```hjson
Items: {
    FraExample: {
        DisplayName: 吉光片羽：示例物
        Tooltip: 寄宿其上的低语流入你的耳中……
        Whisper1: 第一行
        Whisper2: 第二行
    }
}

NPCs: {
    ExampleBoss.DisplayName: 示例之主
}

Buffs: {
    ExampleDebuff: {
        DisplayName: 示例腐蚀
        Description: 你的生命正在被缓慢夺走
    }
}

Projectiles: {
    ExampleProjectile.DisplayName: 示例之矢
}
```

---

## 7. 设定卡模板（写文本用）

### 7.1 碎片

见 `docs/lore/04-fragments.md` §6，复制即可。

### 7.2 神明

见 `docs/lore/02-gods.md` §3。

### 7.3 原创事件

见 `docs/lore/03-events.md` §5。

---

## 8. 模板使用检查

- [ ] 类名、文件名、贴图名、本地化键四者一致。
- [ ] 命名空间与目录一致。
- [ ] `TODO` 全部处理或登记。
- [ ] 没有硬编码中文台词。
- [ ] 贴图存在且路径正确。
- [ ] 构建通过，进游戏可见。
