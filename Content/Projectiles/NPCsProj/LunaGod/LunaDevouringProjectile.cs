using Microsoft.Xna.Framework.Graphics;
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
    public class LunaDevouringProjectile : ModProjectile
    {
        private Vector2 _targetPosition; // 弹幕的目标坐标
        private int _timer; // 计时器

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.timeLeft = 260; // 弹幕存在时间
            Projectile.penetrate = -1; // 无限穿透
            Projectile.tileCollide = false; // 不与方块碰撞
            Projectile.Calamity().DealsDefenseDamage = true;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;//修改弹幕的最大绘制距离
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 每帧更新计时器
            _timer++;

            // 120帧一轮循环
            if (_timer >= 120)
            {
                _timer = 0; // 重置计时器，开始新的循环
            }

            // 前40帧：持续获取位置并指向目标点
            if (_timer < 40)
            {
                // 每帧更新目标点
                _targetPosition = GetTargetPosition(player);

                // 计算弹幕到目标点的方向
                Vector2 directionToTarget = _targetPosition - Projectile.Center;
                directionToTarget.Normalize();

                // 逐渐旋转弹幕的角度，直到指向目标点
                float maxRotationSpeed = 0.2f; // 最大旋转速度（弧度/帧）
                float currentAngle = Projectile.rotation - MathHelper.PiOver2; // 当前角度（减去PiOver2以对齐尖端）
                float targetAngle = directionToTarget.ToRotation(); // 目标角度
                float newAngle = MathHelper.WrapAngle(MathHelper.WrapAngle(targetAngle - currentAngle) * maxRotationSpeed + currentAngle);

                Projectile.rotation = newAngle + MathHelper.PiOver2; // 更新弹幕的旋转角度
            }
            // 第40帧：保存冲刺方向
            else if (_timer == 40)
            {
                // 计算弹幕到目标点的方向
                Vector2 directionToTarget = _targetPosition - Projectile.Center;
                directionToTarget.Normalize();

                // 保存冲刺方向
                _dashDirection = directionToTarget;
            }
            // 后60帧：朝最后确定的方向冲刺
            else if (_timer > 40)
            {
                // 设置弹幕的速度
                Projectile.velocity += _dashDirection * 0.5f;
                // 限制弹幕的最大速度
                if (Projectile.velocity.Length() > 30)
                {
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 30;
                }
                //停下，准备开始新的循环
                if (_timer >= 119)
                {
                    Projectile.velocity = Vector2.Zero; // 重置速度
                }
            }
        }

        // 用于保存冲刺方向的变量
        private Vector2 _dashDirection;
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<LunaLockedDebuff>(), 120);
            target.AddBuff(ModContent.BuffType<Nightwither>(), 180);
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
