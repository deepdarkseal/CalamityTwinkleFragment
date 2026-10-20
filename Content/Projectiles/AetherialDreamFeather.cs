using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles
{
    internal class AetherialDreamFeather : ModProjectile
    {
        // 存储原始伤害值用于计算羽毛伤害
        //private int originalDamage = 0;
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 300; // 5秒存在时间
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            // 羽毛飘落效果
            Projectile.velocity.Y += 0.1f;
            Projectile.velocity.X *= 0.98f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 透明度变化
            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha = 255 - (int)(255 * (Projectile.timeLeft / 30f));
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 反弹效果
            if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X * 0.7f;
            if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y * 0.7f;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            // 计算基础伤害（1000 * 玩家伤害加成 * 伤害系数）
            float baseDamage = 1000f * player.GetTotalDamage(DamageClass.Generic).ApplyTo(1f);
            int featherDamage = (int)baseDamage;

            // 应用伤害
            target.SimpleStrikeNPC(featherDamage, Projectile.direction);

            // 粒子效果
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.WhiteTorch, 0f, 0f, 100, default, 1.5f);
            }

            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            // 粒子效果
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.WhiteTorch, 0f, 0f, 100, default, 1.5f);
            }
        }

        public override bool? CanCutTiles() => false;

        // 移除PostAI方法中的玩家收集检测
    }
}