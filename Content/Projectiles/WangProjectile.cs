using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles
{
    internal class WangProjectile : ModProjectile
    {
        // 添加一个字段来标记是否是幻影弹幕
        public bool IsPhantom = false;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.scale = 2f;
            // 设置独立无敌帧
            Projectile.usesLocalNPCImmunity = true; // 启用独立无敌帧
            Projectile.localNPCHitCooldown = 20; // 独立无敌帧
        }

        public override void AI()
        {
            Projectile.rotation += 0.05f;

            // 只有非幻影弹幕才有跟踪能力
            if (!IsPhantom)
            {
                // 微弱的跟踪
                NPC target = FindClosestNPC(500f);
                if (target != null)
                {
                    Vector2 direction = target.Center - Projectile.Center;
                    direction.Normalize();
                    Projectile.velocity = (Projectile.velocity * 15f + direction * 2f) / 16f;
                }

                // 只有原始弹幕才生成幻影
                if (Projectile.timeLeft % 30 == 0)
                {
                    CreatePhantomProjectile();
                }
            }

            // 逐渐变淡
            if (IsPhantom)
            {
                // 幻影弹幕的淡出效果
                Projectile.alpha = 150 + (int)(Projectile.timeLeft / 120f * 105f);
            }
            else
            {
                // 原始弹幕的淡出效果
                Projectile.alpha = (int)((1f - Projectile.timeLeft / 300f) * 100f);
            }
        }

        private void CreatePhantomProjectile()
        {
            // 创建幻影弹幕
            int phantom = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Projectile.velocity * 0.1f,
                Projectile.type,
                (int)(Projectile.damage * 1f),
                Projectile.knockBack * 0.5f,
                Projectile.owner
            );

            if (phantom < Main.maxProjectiles)
            {
                Projectile phantomProj = Main.projectile[phantom];

                // 标记为幻影弹幕
                if (phantomProj.ModProjectile is WangProjectile phantomWang)
                {
                    phantomWang.IsPhantom = true;
                }

                // 设置幻影弹幕的属性
                phantomProj.timeLeft = 300;
                phantomProj.scale = 2f;
                phantomProj.alpha = 150;
                phantomProj.tileCollide = false;
                phantomProj.penetrate = 1;
                phantomProj.light = 0.9f;

                // 添加随机速度变化
                //phantomProj.velocity = phantomProj.velocity.RotatedByRandom(MathHelper.ToRadians(30));
                //phantomProj.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
            }
        }

        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC target = Main.npc[k];
                if (target.CanBeChasedBy())
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);

                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = target;
                    }
                }
            }

            return closestNPC;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null,
                Color.White * (1f - Projectile.alpha / 255f), Projectile.rotation,
                texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}