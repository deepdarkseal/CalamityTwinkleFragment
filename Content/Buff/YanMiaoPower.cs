using Terraria;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Buff
{
    class YanMiaoPower : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; // 不显示时间
            Main.debuff[Type] = false; // 不是Debuff
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 半血以下时盐喵趴在头上：提高防御与生命再生。
            // 高血量时不做任何扣减——statDefense / lifeRegen 每帧都会重置，
            // 旧的“高血量 -= 20”分支实际上是在惩罚满血玩家。
            if (player.statLife <= player.statLifeMax2 / 2)
            {
                player.statDefense += 20;
                player.lifeRegen += 10;
            }
        }
    }
}
