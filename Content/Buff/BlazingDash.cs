using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Buff
{
    public class BlazingDash : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // 退出游戏时不保存Buff
            Main.debuff[Type] = false;   // 不是负面效果
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 冲刺速度翻倍
            player.runAcceleration *= 2.5f;
            player.maxRunSpeed *= 2.5f;
            // 减少50%冲刺冷却时间
            if (player.dashDelay > 0)
                player.dashDelay = (int)(player.dashDelay * 0.5f);
        }
    }
}