using Calamitytwinklefragment.Content.TFTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.CTFplayer
{
    public class YanMiaoPowerPlayer : ModPlayer
    {
        // 标识是否装备
        public bool hasLicenseYan;
        // 监听玩家受到伤害事件
        public override void OnHurt(Player.HurtInfo info)
        {
            Main.NewText($"1");
            if (hasLicenseYan) 
            {
                Main.NewText($"2");
                ApplyAdrenalineHeal(); 
            }
        }

        // 应用肾上腺素恢复效果
        private void ApplyAdrenalineHeal()
        {
            // 获取肾上腺素值
            float adrenaline = AdrenalineHelper.GetAdrenaline(Player);
            float adrenalineMax = AdrenalineHelper.GetAdrenalineMax(Player);

            if (adrenalineMax <= 0f)
                return;

            // 计算恢复量
            float healRatio = 0.1f * (adrenaline / adrenalineMax);
            int healAmount = (int)(Player.statLifeMax2 * healRatio);
            Main.NewText($"{healAmount}");
            // 恢复血量并播放效果
            Player.statLife += healAmount;
            if (Player.statLife > Player.statLifeMax2)
                Player.statLife = Player.statLifeMax2;

            Player.HealEffect(healAmount);
        }
        public override void ResetEffects()
        {
            hasLicenseYan = false; // 每帧重置装备状态
        }
    }
}