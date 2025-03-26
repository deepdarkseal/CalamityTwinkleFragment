using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace Calamitytwinklefragment.Content.Buff.Debuffs
{
    class LunaLockedDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true; // 这是一个debuff
            Main.buffNoSave[Type] = true; // 退出游戏时不保存该减益
        }
        public override void Update(Player player, ref int buffIndex)
        {
            // 增加玩家受到的伤害
            player.GetDamage<Terraria.ModLoader.DefaultDamageClass>() *= 1.1f;
        }
    }
}
