using System;
using System.Collections.Generic;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.Typeless;
using Calamitytwinklefragment.Content.Buff;
using Microsoft.Xna.Framework; // 使用 XNA 的 Vector2
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Accessories.DragonfireAmberPauldron
{
    public class DragonfirePauldronPlayer : ModPlayer
    {
        // 标识是否装备了龙火涎香肩甲
        public bool hasDragonfirePauldron;

        // 存储冲刺路径坐标
        public List<Vector2> dashPath = [];

        // 最大路径点数
        private const int DashPathMaxLength = 30;

        // 冲刺状态的辅助变量

        public override void ResetEffects()
        {
            hasDragonfirePauldron = false; // 每帧重置装备状态
        }

        public override void PostUpdateEquips()
        {
            if (hasDragonfirePauldron)
            {
                // 冲刺速度提升32%
                Player.runAcceleration *= 1.1f;
                Player.maxRunSpeed *= 1.1f;

                // 冲刺间隔减少33%
                if (Player.dashDelay > 0)
                    Player.dashDelay = (int)(Player.dashDelay * 1f);
            }
        }

        public override void PostUpdate()
        {
            if (hasDragonfirePauldron && Player.dashDelay == -1)
            {
                // 记录路径点（每帧添加当前位置）
                dashPath.Add(Player.Center);
                if (dashPath.Count > DashPathMaxLength)
                    dashPath.RemoveAt(0);

                // 每隔3帧生成特效
                if (Player.miscCounter % 3 == 0)
                {
                    SpawnWaveAndExplosionEffects();
                }

                // 碰撞敌人施加双减益
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && npc.Hitbox.Intersects(Player.Hitbox))
                    {
                        npc.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊压力
                        npc.AddBuff(ModContent.BuffType<Dragonfire>(), 300); // 龙焰
                        Player.AddBuff(ModContent.BuffType<BlazingDash>(), 120); // 持续2秒
                    }
                }
            }

        }

        // 生成爆炸投射物
        private void SpawnWaveAndExplosionEffects()
        {
            foreach (Vector2 pos in dashPath)
            {
                // 生成深渊碎波（海浪）
                int damage = Player.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(300));
                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    pos,
                    Vector2.Zero,
                    ModContent.ProjectileType<AbyssalCrushWave>(),
                    damage, // 伤害
                    0f,
                    Player.whoAmI
                );

                // 生成龙焰爆破（爆炸）
                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    pos,
                    Vector2.Zero,
                    ModContent.ProjectileType<DragonfireExplosion>(),
                    damage,
                    0f,
                    Player.whoAmI
                );
            }
        }
    }
}