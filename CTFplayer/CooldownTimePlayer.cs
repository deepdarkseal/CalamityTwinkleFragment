using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.CTFplayer
{
    class CooldownTimePlayer : ModPlayer
    {
        public int cooldownTimer = 0; // 冷却计时器

        public override void PostUpdate()
        {
            // 每帧减少冷却时间
            if (cooldownTimer > 0)
            {
                cooldownTimer--;
            }
        }
    }
}
