using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod;

namespace Calamitytwinklefragment.Content.Projectiles
{
    internal class DeepRedExplosion : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 100; // 爆炸范围宽度
            Projectile.height = 100; // 爆炸范围高度
            Projectile.friendly = true; // 对玩家友好
            Projectile.hostile = false; // 不对玩家敌对
            Projectile.penetrate = -1; // 穿透
            Projectile.timeLeft = 60; // 弹幕存在时间
            Projectile.tileCollide = false; // 不与方块碰撞
            Projectile.ignoreWater = true; // 忽略水
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>(); // 伤害类型为真近战

            // 设置独立无敌帧
            Projectile.usesLocalNPCImmunity = true; // 启用独立无敌帧
            Projectile.localNPCHitCooldown = 15; // 独立无敌帧
        }
        public override void AI()
        {
            // 在弹幕存在期间生成粒子效果
            if (Projectile.timeLeft > 5)
            {
                // 生成 破片
                for (int i = 0; i < 4; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * 10f; // 随机方向
                    Dust.NewDustPerfect(
                        Projectile.Center, // 破片生成位置
                        ModContent.DustType<DeepRedFragment>(), // 破片弹幕类型
                        velocity, // 破片速度
                        0, default, // 击退
                        2f
                    );
                }
            }
            // 播放爆炸音效
            if (Projectile.timeLeft == 10)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            }
        }
    }
}
