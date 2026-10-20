using System;
using System.Collections.Generic;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Calamitytwinklefragment.CTFplayer;
using Calamitytwinklefragment.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 月光大剑：致敬《黑暗之魂》系列的月光大剑，攻击模式参考《艾尔登法环》的暗月大剑。
    /// 左键为三段连斩，附魔期间每段都会释放月光波；右键消耗魔力附魔；附魔期间右键为重击，可按住自动挥舞。
    /// </summary>
    internal class MoonlightGreatsword : ModItem
    {
        public const int MoonlightManaCost = 80;
        public const int MoonlightDuration = 45 * 60;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            // 允许按住右键连续重击（附魔期间的月光波也能自动挥舞）。
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 1400;
            Item.DamageType = DamageClass.Melee;
            Item.width = 180;
            Item.height = 180;
            Item.scale = 0.62f;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = false;
            Item.knockBack = 8f;
            Item.crit = 4;
            Item.value = Item.sellPrice(0, 30, 0, 0);
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<MoonlightGreatswordSwing>();
            Item.shootSpeed = 16f;
            Item.UseSound = SoundID.Item1;
            Item.noMelee = true;
            Item.noUseGraphic = false;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            MoonlightGreatswordPlayer modPlayer = player.GetModPlayer<MoonlightGreatswordPlayer>();

            if (player.altFunctionUse == 2)
            {
                if (modPlayer.MoonlightActive)
                {
                    // 附魔期间右键：缓慢的重击，剑锋会释放大型月光波；可按住右键自动挥舞。
                    Item.useStyle = ItemUseStyleID.Swing;
                    Item.useTime = 30;
                    Item.useAnimation = 30;
                    Item.autoReuse = true;
                    Item.useTurn = false;
                    Item.noMelee = true;
                    Item.shoot = ModContent.ProjectileType<MoonlightGreatswordSwing>();
                    Item.shootSpeed = 16f;
                    Item.knockBack = 11f;
                    Item.UseSound = SoundID.Item1 with { Pitch = -0.35f, Volume = 1.1f };
                    return true;
                }

                int manaCost = GetManaCost(player);
                if (player.statMana < manaCost)
                    return false;

                // 未附魔时右键：举剑凝聚月光。
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 40;
                Item.useAnimation = 40;
                Item.autoReuse = true;
                Item.useTurn = false;
                Item.noMelee = true;
                Item.shoot = ModContent.ProjectileType<MoonlightGreatswordHeld>();
                Item.shootSpeed = 1f;
                Item.knockBack = 0f;
                Item.UseSound = SoundID.Item43 with { Pitch = -0.2f };
                return true;
            }

            // 左键：三段连斩。第三段更慢、更重。
            int combo = modPlayer.ComboIndex;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = combo == 2 ? 24 : 18;
            Item.useAnimation = combo == 2 ? 24 : 18;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<MoonlightGreatswordSwing>();
            Item.shootSpeed = 16f;
            Item.knockBack = combo == 2 ? 10f : 8f;
            Item.UseSound = SoundID.Item1;
            return true;
        }

        public override bool? UseItem(Player player)
        {
            MoonlightGreatswordPlayer modPlayer = player.GetModPlayer<MoonlightGreatswordPlayer>();

            if (player.altFunctionUse == 2 && !modPlayer.MoonlightActive)
            {
                int manaCost = GetManaCost(player);
                if (player.statMana < manaCost)
                    return false;

                // 使用期间隐藏原版手持贴图，交给挥砍/举剑弹幕绘制。
                // 举剑动画比物品使用动画更长，因此这里单独给足时间。
                modPlayer.HideItemGraphicTimer = Math.Max(modPlayer.HideItemGraphicTimer, 56);

                player.statMana -= manaCost;
                player.manaRegenDelay = Math.Max(player.manaRegenDelay, 90);
                modPlayer.ActivateMoonlight();

                // 直接生成举剑动画；右手使用只会在本地客户端执行。
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    player.MountedCenter,
                    Vector2.Zero,
                    ModContent.ProjectileType<MoonlightGreatswordHeld>(),
                    0,
                    0f,
                    player.whoAmI);

                // 四周的月光向剑身汇聚。
                for (int i = 0; i < 36; i++)
                {
                    float angle = MathHelper.TwoPi * i / 36f;
                    Vector2 offset = angle.ToRotationVector2() * 96f;

                    Dust dust = Dust.NewDustPerfect(
                        player.MountedCenter + offset,
                        DustID.Frost,
                        -offset * 0.055f,
                        100,
                        new Color(150, 195, 255),
                        1.35f);
                    dust.noGravity = true;
                }
            }
            else
            {
                // 使用期间隐藏原版手持贴图，交给挥砍/举剑弹幕绘制。
                modPlayer.HideItemGraphicTimer = Math.Max(modPlayer.HideItemGraphicTimer, Item.useAnimation + 2);
            }

            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            MoonlightGreatswordPlayer modPlayer = player.GetModPlayer<MoonlightGreatswordPlayer>();

            // 右键附魔：举剑动画已在 UseItem 中生成，这里阻止原版生成额外弹幕。
            if (type == ModContent.ProjectileType<MoonlightGreatswordHeld>())
                return false;

            // 开始新的攻击时移除仍在淡出的举剑动画，避免同时出现两把剑。
            int heldType = ModContent.ProjectileType<MoonlightGreatswordHeld>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.owner == player.whoAmI && projectile.type == heldType)
                    projectile.Kill();
            }

            // 同一时间只保留一把挥砍弹幕，避免旧挥砍与新挥砍造成重复伤害。
            int swingType = ModContent.ProjectileType<MoonlightGreatswordSwing>();
            int previousSwing = modPlayer.ActiveSwingProjectile;
            if (previousSwing >= 0 && previousSwing < Main.maxProjectiles)
            {
                Projectile oldSwing = Main.projectile[previousSwing];
                if (oldSwing.active && oldSwing.owner == player.whoAmI && oldSwing.type == swingType)
                    oldSwing.Kill();
            }

            Vector2 aim = velocity;
            if (aim.LengthSquared() < 0.01f)
                aim = Vector2.UnitX * player.direction;
            aim.Normalize();

            bool heavy = player.altFunctionUse == 2 && modPlayer.MoonlightActive;
            int combo = modPlayer.ComboIndex;
            int mode = heavy ? 3 : combo;
            float aimAngle = aim.ToRotation();

            // 挥砍弹幕同时承担伤害判定，判定线会跟随剑身旋转。
            int swing = Projectile.NewProjectile(
                source,
                player.MountedCenter,
                Vector2.Zero,
                type,
                damage,
                knockback,
                player.whoAmI,
                mode,
                aimAngle,
                player.direction);

            if (swing >= 0 && swing < Main.maxProjectiles)
            {
                int animationTime = player.itemAnimationMax > 0 ? player.itemAnimationMax : Item.useAnimation;
                Main.projectile[swing].timeLeft = Math.Max(10, animationTime);
                modPlayer.ActiveSwingProjectile = swing;
            }

            // 附魔期间所有攻击都会释放月光波；第三段与重击的波更大。
            if (modPlayer.MoonlightActive)
            {
                float waveDamageMultiplier;
                float waveScale;
                int waveTime;

                if (heavy)
                {
                    waveDamageMultiplier = 1.2f;
                    waveScale = 1.45f;
                    waveTime = 110;
                }
                else if (combo == 2)
                {
                    waveDamageMultiplier = 0.9f;
                    waveScale = 1.25f;
                    waveTime = 100;
                }
                else
                {
                    waveDamageMultiplier = 0.55f;
                    waveScale = 0.95f;
                    waveTime = 85;
                }

                int waveDamage = Math.Max(1, (int)(damage * waveDamageMultiplier));
                Vector2 wavePosition = player.MountedCenter + aim * 64f;

                int wave = Projectile.NewProjectile(
                    source,
                    wavePosition,
                    aim * Item.shootSpeed,
                    ModContent.ProjectileType<MoonLightBeam>(),
                    waveDamage,
                    knockback * 0.8f,
                    player.whoAmI,
                    heavy ? 1f : 0f);

                if (wave >= 0 && wave < Main.maxProjectiles)
                {
                    Main.projectile[wave].scale = waveScale;
                    Main.projectile[wave].timeLeft = waveTime;
                }
            }

            if (!heavy)
            {
                // 推进连斩段数。
                modPlayer.ComboIndex = (modPlayer.ComboIndex + 1) % 3;
                modPlayer.ComboTimer = 90;
            }

            return false;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            MoonlightGreatswordPlayer modPlayer = player.GetModPlayer<MoonlightGreatswordPlayer>();

            if (modPlayer.MoonlightActive)
                damage *= 1.3f;

            if (modPlayer.MoonlightActive && player.altFunctionUse == 2)
                damage *= 1.25f; // 附魔重击
            else if (player.altFunctionUse != 2 && modPlayer.ComboIndex == 2)
                damage *= 1.45f; // 第三段连斩
        }

        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            if (player.GetModPlayer<MoonlightGreatswordPlayer>().MoonlightActive)
                crit += 8f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            MoonlightGreatswordPlayer modPlayer = Main.LocalPlayer.GetModPlayer<MoonlightGreatswordPlayer>();
            if (!modPlayer.MoonlightActive)
                return;

            int seconds = (modPlayer.MoonlightTime + 59) / 60;
            tooltips.Add(new TooltipLine(
                Mod,
                "MoonlightTime",
                Language.GetTextValue("Mods.Calamitytwinklefragment.Items.MoonlightGreatsword.MoonlightTime", seconds))
            {
                OverrideColor = new Color(150, 205, 255)
            });
        }

        public override void HoldItem(Player player)
        {
            MoonlightGreatswordPlayer modPlayer = player.GetModPlayer<MoonlightGreatswordPlayer>();
            Item.noUseGraphic = modPlayer.HideItemGraphicTimer > 0 || player.itemAnimation > 0;
        }

        private static int GetManaCost(Player player)
        {
            return Math.Max(1, (int)(MoonlightManaCost * player.manaCost));
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<UelibloomBar>(), 12);
            recipe.AddIngredient(ModContent.ItemType<MeldBlob>(), 8);
            recipe.AddIngredient(ItemID.SoulofLight, 10);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}
