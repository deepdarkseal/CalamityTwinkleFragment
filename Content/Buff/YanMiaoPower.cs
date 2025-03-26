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
            // 半血以下时激活防御和生命再生
            if (player.statLife <= player.statLifeMax2 / 2)
            {
                player.statDefense += 20;
                player.lifeRegen += 5;
            }
        }
    }
}