using System;
using System.Collections.Generic;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Calamitytwinklefragment.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 狩猎大蛇矛：致敬《艾尔登法环》的同名武器。
    /// 左键横扫，右键向前突刺。
    /// 只有在三族巨蛇在场时，剑气才会附着在矛身上，作为矛的延长段参与判定；
    /// 巨蛇不在场时它只是一柄普通长矛。
    /// </summary>
    internal class SerpentHunter : ModItem
    {
        /// <summary>剑气（矛的延长段）对巨蛇的伤害倍率。</summary>
        public const float WormDamageMultiplier = 3f;

        // Calamity 用类名区分体节（Head / Body1 / Body2 / Tail），所以按族名前缀匹配整个族系，
        // 而不是把每个体节类型 ID 写死；新增体节时无需改这里。
        private static readonly string[] WormBossPrefixes =
        [
            "StormWeaver",      // 风暴编织者
            "DevourerofGods",   // 神明吞噬者
            "Thanatos"          // 塔纳托斯
        ];

        // NPC.type -> 是否属于上述巨蛇族系。类型在加载期固定，运行期查表即可，避免每次判定都反射。
        private static readonly Dictionary<int, bool> WormNpcCache = new();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            // 原版默认禁止按住右键连续使用；突刺需要能连续触发。
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 666;
            Item.DamageType = DamageClass.Melee;
            Item.width = 160;
            Item.height = 160;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 10f;
            Item.crit = 4;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;
            Item.noMelee = false;      // 无巨蛇时由原版近战判定
            Item.noUseGraphic = false; // 用原版持矛姿势
            Item.shoot = ModContent.ProjectileType<SerpentHunterStorm>();
            Item.shootSpeed = 16f;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            // 只在攻击动画开始时决定动作与判定方式；动画中途巨蛇死亡不应切换判定。
            if (player.itemAnimation <= 0)
            {
                bool wormsPresent = IsWormBossPresent();

                if (player.altFunctionUse == 2)
                {
                    // 右键：向前刺击，剑气会沿矛身伸得更远。
                    Item.useStyle = ItemUseStyleID.Shoot;
                    Item.useTime = 32;
                    Item.useAnimation = 32;
                    Item.knockBack = 12f;
                    Item.UseSound = SoundID.Item1 with { Pitch = -0.2f };
                }
                else
                {
                    // 左键：横扫，剑气随矛身一起扫过。
                    Item.useStyle = ItemUseStyleID.Swing;
                    Item.useTime = 24;
                    Item.useAnimation = 24;
                    Item.knockBack = 10f;
                    Item.UseSound = SoundID.Item1;
                }

                // 有剑气时判定完全交给弹幕，避免原版近战框与剑气重复命中同一目标。
                Item.noMelee = wormsPresent;
            }

            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 巨蛇不在场：不生成剑气，本次攻击交给原版近战判定。
            if (!IsWormBossPresent())
                return false;

            // 与 CanUseItem 保持一致：本次攻击由剑气判定，关闭原版近战框。
            Item.noMelee = true;

            // 同一玩家只保留最新的一道附着剑气，避免快速连击时多段判定叠加。
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile old = Main.projectile[i];
                if (old.active && old.owner == player.whoAmI && old.type == type)
                    old.Kill();
            }

            bool thrust = player.altFunctionUse == 2;

            Projectile.NewProjectile(
                source,
                player.MountedCenter,
                Vector2.Zero,
                type,
                damage,
                knockback,
                player.whoAmI,
                thrust ? 1f : 0f,          // ai[0]：0 横扫 / 1 突刺
                velocity.ToRotation());    // ai[1]：首帧兜底角度，AI 之后会同步 itemRotation

            return false;
        }

        /// <summary>
        /// 判断目标是否属于三族巨蛇的任意体节。
        /// 用运行时类型名做前缀匹配，可以一并覆盖头、身、尾以及后续新增的体节变体。
        /// </summary>
        public static bool IsWormBoss(NPC npc)
        {
            if (npc == null || !npc.active)
                return false;

            if (WormNpcCache.TryGetValue(npc.type, out bool cached))
                return cached;

            bool result = false;
            if (npc.ModNPC != null)
            {
                string typeName = npc.ModNPC.GetType().Name;
                foreach (string prefix in WormBossPrefixes)
                {
                    if (typeName.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        result = true;
                        break;
                    }
                }
            }

            WormNpcCache[npc.type] = result;
            return result;
        }

        /// <summary>世界中是否存在三族巨蛇的任意体节。</summary>
        public static bool IsWormBossPresent()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && IsWormBoss(npc))
                    return true;
            }

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.LunarBar, 13).// 夜明锭
                AddIngredient(ModContent.ItemType<ArmoredShell>(), 1).// 装甲外壳
                AddIngredient(ModContent.ItemType<DarkPlasma>(), 1).// 暗离子体
                AddIngredient(ModContent.ItemType<TwistingNether>(), 1).// 扭曲虚空
                Register();
        }
    }
}
