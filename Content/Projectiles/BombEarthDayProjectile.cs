using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Projectiles.DraedonsArsenal;
using Terraria.Audio;
using CalamityMod.Projectiles.Melee;
using Mono.Cecil;
using static System.Net.Mime.MediaTypeNames;

namespace Calamitytwinklefragment.Content.Projectiles
{
    internal class BombEarthDayProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 180;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>(); 
        }

        public override void AI()
        {
            if (Projectile.velocity.X != 0f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            else Projectile.rotation = MathHelper.Pi;

            if (Projectile.velocity.Y < 12f)
                Projectile.velocity.Y += 0.15f;

            // 生成一些粉色的粒子，显示弹幕正在吸收能量
            if (!Main.dedServ)
            {
                for (int i = 0; i < 2; i++)
                {
                    float offset = Main.rand.NextFloat(38f, 42f);
                    if (Projectile.Calamity().stealthStrike)
                        offset *= 1.66f;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2CircularEdge(offset, offset), 267); // 267 是粉色粒子的ID
                    dust.velocity = Projectile.DirectionFrom(dust.position) * offset / 12f + Projectile.velocity;
                    dust.noGravity = true;
                    dust.color = Color.HotPink; // 设置粒子颜色为粉色
                    dust.scale = 1.2f; // 调整粒子大小
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(TeslaCannon.FireSound, Projectile.Center);
            if (Main.myPlayer == Projectile.owner)
            {
                if (!Projectile.Calamity().stealthStrike)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Projectile explosion = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<WavePounderBoom>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        explosion.ai[1] = Main.rand.NextFloat(110f, 240f) + i * 20f; // 随机化最大半径
                        explosion.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f); // 随机化插值步长
                        explosion.netUpdate = true;
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Projectile explosion = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<WavePounderBoom>(), (int)(Projectile.damage * 0.6), Projectile.knockBack, Projectile.owner);
                        if (explosion.whoAmI.WithinBounds(Main.maxProjectiles))
                        {
                            explosion.ai[1] = Main.rand.NextFloat(240f, 600f) + i * 45f; // 随机化最大半径
                            explosion.localAI[1] = Main.rand.NextFloat(0.08f, 0.25f); // 随机化插值步长
                            explosion.Opacity = MathHelper.Lerp(0.18f, 0.6f, i / 7f) + Main.rand.NextFloat(-0.08f, 0.08f);
                            explosion.Calamity().stealthStrike = true;
                            explosion.netUpdate = true;
                        }
                    }
                }
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PinkFish>(),0, default, Projectile.owner);
            }
            // 检测世界种子是GFB
            if (Main.zenithWorld)
            {
                // 关闭游戏程序
                Environment.Exit(0);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 最大弹跳次数
            int maxBounces = 3;

            // 初始化弹跳计数器（存储在Projectile.ai[0]）
            if (Projectile.ai[0] == 0)
                Projectile.ai[0] = maxBounces;

            // 弹跳逻辑
            if (Projectile.ai[0] > 0)
            {
                // 碰撞方向判断
                if (Math.Abs(oldVelocity.Y) > Math.Abs(oldVelocity.X))
                {
                    Projectile.velocity.Y = -oldVelocity.Y * 0.6f; // 垂直反弹（Y方向速度反向并衰减）
                    SoundEngine.PlaySound(SoundID.Item56, Projectile.Center); // 播放弹跳音效
                }
                else
                {
                    Projectile.velocity.X = -oldVelocity.X * 0.6f; // 水平反弹（X方向速度反向并衰减）
                }

                Projectile.ai[0]--; // 减少剩余弹跳次数
                return false; // 不销毁弹幕
            }
            else
            {
                Projectile.velocity = Vector2.Zero; // 弹跳次数用尽后停止移动
                return false; // 仍不销毁弹幕（若需销毁可改为 true）
            }
        }
    }
}
