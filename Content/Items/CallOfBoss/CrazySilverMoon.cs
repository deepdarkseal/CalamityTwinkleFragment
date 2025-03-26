using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Calamitytwinklefragment.Content.NPCs.BossGod;

namespace Calamitytwinklefragment.Content.Items.CallOfBoss
{
    class CrazySilverMoon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.maxStack = 20;
            Item.value = 0;
            Item.rare = ItemRarityID.Purple;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }
        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {

                SoundEngine.PlaySound(SoundID.Roar, player.position);//播放吼叫音效
                int type = ModContent.NPCType<LunaGoddessLunatica>();
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, type);//生成Boss
                }
                else
                {
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);//发包，用来联机同步
                }
            }
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            // 检查当前时间是否为夜晚或日食
            bool isNight = !Main.dayTime; // 夜晚
            bool isEclipse = Main.eclipse; // 日食

            // 检查世界上是否存在该Boss，玩家是否在神圣地，以及时间是否为夜晚或日食
            return !NPC.AnyNPCs(ModContent.NPCType<LunaGoddessLunatica>()) && (isNight || isEclipse);
        }
    }
}
