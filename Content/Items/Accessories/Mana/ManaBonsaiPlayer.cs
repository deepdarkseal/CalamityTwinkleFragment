using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Accessories.Mana
{
    public class ManaBonsaiPlayer : ModPlayer
    {
        // 定义一个字段，用于存储是否启用暴击率增加
        public bool enableCritBonus = false;

        public override void ResetEffects()
        {
            // 在每个游戏帧开始时重置字段
            enableCritBonus = false;
        }

        public override void UpdateEquips()
        {
            // 如果启用了暴击率增加
            if (enableCritBonus)
            {
                
                // 基于总魔力增加暴击率
                float critChanceIncrease = Player.statManaMax2 * 0.01f;
                Player.GetCritChance<MagicDamageClass>() += critChanceIncrease;
            }
        }
    }
}