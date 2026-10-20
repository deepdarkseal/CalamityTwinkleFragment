using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Buff
{
    public class Cutting : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true; // 设置为减益
            Main.pvpBuff[Type] = true; // 在PvP中生效
            Main.buffNoSave[Type] = true; // 不保存减益状态
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // 降低敌人10%接触伤害
            npc.GetGlobalNPC<CuttingNPC>().cutting = true;
        }
    }
    public class CuttingNPC : GlobalNPC
    {
        public bool cutting = false; // 实例字段

        public override bool InstancePerEntity => true; // 设置为 true

        public override void ResetEffects(NPC npc)
        {
            cutting = false; // 每帧重置状态
        }


            public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
            {
                if (cutting)
                {
                    modifiers.FinalDamage *= 0.9f; // 降低10%接触伤害
                }
            }
    }
    
}
