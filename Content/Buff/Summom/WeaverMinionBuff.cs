using Calamitytwinklefragment.Content.Projectiles.Summom;
using Calamitytwinklefragment.CTFplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Buff.Summom
{
    // 召唤物Buff类
    public class WeaverMinionBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<WeaverMinion>()] > 0)
            {
                player.GetModPlayer<CTFMinionPlayer>().weaverMinion = true;
            }

            if (!player.GetModPlayer<CTFMinionPlayer>().weaverMinion)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
