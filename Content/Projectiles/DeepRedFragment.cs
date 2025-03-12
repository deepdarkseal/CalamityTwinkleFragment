using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using CalamityMod;

namespace Calamitytwinklefragment.Content.Projectiles
{
    internal class DeepRedFragment : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            // 设置 Dust 的初始属性
            dust.noGravity = true; // 不受重力影响
            dust.noLight = false; // 发出光
            dust.scale = 1.5f; // 初始大小
            dust.alpha = 0; // 透明度（0为不透明）
        }

        public override bool Update(Dust dust)
        {
            // 逐渐减速
            dust.velocity *= 0.8f;

            // 根据速度旋转
            dust.rotation = dust.velocity.ToRotation();

            // 逐渐缩小
            dust.scale -= 0.02f;

            // 如果 Dust 缩小到一定程度，则消失
            if (dust.scale < 0.1f)
            {
                dust.active = false;
            }

            // 生成额外的粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust newDust = Dust.NewDustPerfect(dust.position, DustID.FireworkFountain_Red, dust.velocity * 0.5f, 0, default, 1.5f);
                newDust.noGravity = true;
            }

            // 返回 true 表示 Dust 仍然活跃
            return false;
        }
    }
}
