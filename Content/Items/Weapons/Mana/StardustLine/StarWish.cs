using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Rarities;
using Calamitytwinklefragment.Content.Projectiles;
using Calamitytwinklefragment.CTFplayer;
using Calamitytwinklefragment.System;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;
using static Terraria.ModLoader.ModContent;

namespace Calamitytwinklefragment.Content.Items.Weapons.Mana.StardustLine
{
    internal class StarWish : ModItem
    {
        // 右键冷却时间
        private static readonly int CooldownTime = 600; // 10秒冷却（60 ticks = 1秒）
        private static float starf = 0.5f;
        public override void SetDefaults()
        {
            Item.width = 100;
            Item.height = 100;
            Item.damage = 812; // 基础伤害
            Item.DamageType = DamageClass.Magic; // 魔法武器
            Item.mana = 20; // 每次使用消耗的魔力值
            Item.useTime = 45; // 使用间隔
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot; // 射击类型
            Item.noMelee = true; // 不是近战武器
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 30); // 物品价值
            Item.rare = ModContent.RarityType<Violet>(); // 稀有度
            Item.autoReuse = true; // 自动连发
            Item.shoot = ProjectileType<WishStar>(); // 默认发射的弹幕
            Item.shootSpeed = 30f; // 弹幕速度
            Item.staff[Item.type] = true;
        }
        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            // 如果击败了 Yharon，增加暴击率
            if (DownedBossSystem.downedYharon)
            {
                starf = 0.6f;
            }
            if (DownedBossSystem.downedExoMechs)
            {
                crit += 10f; // 增加暴击率
            }
        }
        // 左键发射星星弹幕
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2) // 左键
            {
                int starCount = 8; // 发射的星星数量
                float spread = MathHelper.ToRadians(Main.rand.NextFloat(10f, 60f)); // 随机角度间隔;
                for (int i = 0; i < starCount; i++)
                {
                    // 随机速度（10~25）
                    float starSpeed = Main.rand.NextFloat(10f, 40f);

                    // 计算弹幕方向
                    Vector2 starVelocity = velocity.RotatedBy(-spread + i * (spread * 2 / (starCount - 1))).SafeNormalize(Vector2.Zero) * starSpeed;

                    // 发射弹幕
                    Projectile.NewProjectile(
                        source,
                        position,
                        starVelocity,
                        ProjectileType<WishStar>(),
                        (int)(damage * starf),
                        knockback,
                        player.whoAmI
                    );
                }

                // 播放音效
                SoundEngine.PlaySound(SoundID.Item9, player.Center);

                return false;
            }
            return false;
        }

        // 右键发射黑洞弹幕
        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            // 获取当前玩家的 ModPlayer 实例
            CooldownTimePlayer modPlayer = player.GetModPlayer<CooldownTimePlayer>();
            if (player.altFunctionUse == 2) // 右键
            {
                // 检查冷却状态
                if (modPlayer.cooldownTimer > 0)
                {
                    // 查找玩家当前的黑洞弹幕
                    foreach (Projectile proj in Main.projectile)
                    {
                        if (proj.active && proj.owner == player.whoAmI && proj.type == ProjectileType<StarWishBlackHole>())
                        {
                            // 提前引爆逻辑
                            proj.timeLeft = 30;
                            player.Teleport(proj.Center, TeleportationStyleID.TeleportationPotion);
                            SoundEngine.PlaySound(SoundID.Item8, player.Center);
                            player.immune = true;
                            player.immuneTime = 45;
                            return true;
                        }
                    }
                    return false; // 没有黑洞时禁用右键
                }

                // 检查魔力值（显示更精准的数值处理）
                int requiredMana = 500;
                if (player.statMana >= requiredMana)
                {
                    // 获取合法的弹幕来源
                    var source = player.GetSource_ItemUse(Item);

                    // 计算实际伤害（魔法伤害加成）
                    int holedamage = (int)(812 * 1f * player.GetDamage(DamageClass.Magic).ApplyTo(1f));

                    // 发射黑洞弹幕
                    if (Projectile.NewProjectile(
                        source,
                        player.Center,
                        Vector2.Zero,
                        ProjectileType<StarWishBlackHole>(),
                        holedamage,
                        0,
                        player.whoAmI
                    ) == Main.maxProjectiles)
                    {
                        // 弹幕发射失败时的保护逻辑
                        return false;
                    }

                    // 更新状态
                    modPlayer.cooldownTimer = CooldownTime;
                    player.statMana -= requiredMana;
                    SoundEngine.PlaySound(SoundID.Item15, player.Center);
                    return true;
                }
                return false; // 魔力不足
            }
            return true; // 左键正常使用
        }
        public override void UpdateInventory(Player player)
        {
            // 获取当前玩家的 ModPlayer 实例
            CooldownTimePlayer modPlayer = player.GetModPlayer<CooldownTimePlayer>();
            if (modPlayer.cooldownTimer == 1)
                SoundEngine.PlaySound(SoundID.Item35, player.Center);
        }

        
    }
}
