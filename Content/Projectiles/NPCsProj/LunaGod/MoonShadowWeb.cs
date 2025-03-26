using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Calamitytwinklefragment.Content.Buff.Debuffs;
using CalamityMod.Buffs.DamageOverTime;
using Terraria.ID;
using CalamityMod;

namespace Calamitytwinklefragment.Content.Projectiles.NPCsProj.LunaGod
{
    class MoonShadowWeb : ModProjectile
    {
        private Vector2 _targetPosition; // 弹幕的目标坐标
        private int _targetUpdateTimer; // 目标点更新计时器

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.hostile = true;
            Projectile.timeLeft = 600; // 弹幕存在时间
            Projectile.Calamity().DealsDefenseDamage = true;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;//修改弹幕的最大绘制距离
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 每60帧更新一次目标点
            if (_targetUpdateTimer++ >= 60)
            {
                _targetUpdateTimer = 0; // 重置计时器
                _targetPosition = GetTargetPosition(player); // 获取新的目标点
            }

            // 计算弹幕到目标点的方向
            Vector2 directionToTarget = _targetPosition - Projectile.Center;
            directionToTarget.Normalize();

            // 计算当前速度向量的角度
            float currentAngle = Projectile.velocity.ToRotation();
            float targetAngle = directionToTarget.ToRotation();

            // 计算角度差
            float angleDifference = MathHelper.WrapAngle(targetAngle - currentAngle);

            // 逐渐旋转速度向量角度，直到指向目标点
            float maxRotationSpeed = 0.1f; // 最大旋转速度（弧度/帧）
            float newAngle = currentAngle + MathHelper.Clamp(angleDifference, -maxRotationSpeed, maxRotationSpeed);
            Projectile.velocity = newAngle.ToRotationVector2() * Projectile.velocity.Length();

            // 设置弹幕的速度
            float speed = Main.expertMode ? 7.5f : 5f; // 专家和普通
            if (Main.masterMode)
            {
                speed = 10f; // 大师模式
            }
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * speed;

            // 可选：添加旋转效果
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override void OnHitPlayer(Player target,Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<Nightwither>(), 180);
            target.AddBuff(ModContent.BuffType<LunaLockedDebuff>(), 180);
            target.AddBuff(BuffID.Webbed, 30);
        }
        private static Vector2 GetTargetPosition(Player player)
        {
            // 检查玩家是否有“锁定”debuff
            if (player.HasBuff(ModContent.BuffType<Buff.Debuffs.LunaLockedDebuff>()))
            {
                return player.Center; // 直接锁定玩家
            }

            // 根据难度调整随机范围
            float range = Main.expertMode ? 300f : 400f; // 专家和普通
            if (Main.masterMode)
            {
                range = 200f; // 大师模式
            }

            Vector2 randomOffset = new(Main.rand.NextFloat(-range, range), Main.rand.NextFloat(-range, range));
            return player.Center + randomOffset;
        }
    }
}