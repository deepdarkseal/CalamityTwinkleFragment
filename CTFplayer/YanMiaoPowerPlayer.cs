using System;
using CalamityMod;
using Calamitytwinklefragment.Content.Buff;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.CTFplayer
{
    /// <summary>
    /// 盐喵之力的受伤回复：把受伤时损失的肾上腺素转化为生命值。
    /// Calamity 扣减肾上腺素可能比受伤事件晚一两帧，所以在 PostUpdate 里逐帧比较，
    /// 而不是在 OnHurt 当帧直接读取（那时肾上腺素往往还没扣）。
    /// </summary>
    public class YanMiaoPowerPlayer : ModPlayer
    {
        /// <summary>损失全部肾上腺素（100 点）时恢复的最大生命比例。</summary>
        private const float HealRatioPerFullAdrenaline = 0.25f;

        /// <summary>受伤后等待肾上腺素扣减的窗口（帧）。</summary>
        private const int AdrenalineLossWindow = 180;

        /// <summary>调试开关：治疗触发时在聊天栏输出损失量与治疗量。</summary>
        private const bool DebugHealMessage = true;

        private float lastAdrenaline;
        private int lossWindowTimer;
        private bool adrenalineInitialized;

        public override void OnHurt(Player.HurtInfo info)
        {
            // 受伤后开一个窗口，等 Calamity 扣减肾上腺素；窗口内只结算一次。
            lossWindowTimer = AdrenalineLossWindow;
        }

        public override void PostUpdate()
        {
            float currentAdrenaline = Player.Calamity().adrenaline;

            // 首次运行只记录基准值，避免把初始 0 → 当前值误判成“损失”。
            if (!adrenalineInitialized)
            {
                lastAdrenaline = currentAdrenaline;
                adrenalineInitialized = true;
                return;
            }

            if (!Player.active || Player.dead)
            {
                lossWindowTimer = 0;
                lastAdrenaline = currentAdrenaline;
                return;
            }

            // 只有装备了许可证（由饰品每帧刷新盐喵之力）时才结算。
            if (!Player.HasBuff(ModContent.BuffType<YanMiaoPower>()))
            {
                lossWindowTimer = 0;
                lastAdrenaline = currentAdrenaline;
                return;
            }

            if (lossWindowTimer > 0)
                lossWindowTimer--;

            float lost = lastAdrenaline - currentAdrenaline;

            // 只在“刚受过伤 + 肾上腺素确实减少 + 不是主动激活消耗”时结算。
            if (lost > 0.01f && lossWindowTimer > 0 && !Player.Calamity().adrenalineModeActive)
            {
                HealFromLostAdrenaline(lost);
                lossWindowTimer = 0;
            }

            lastAdrenaline = currentAdrenaline;
        }

        private void HealFromLostAdrenaline(float lost)
        {
            // 多人时由服务端结算，客户端只接收血量同步，避免双端各加一次。
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            float adrenalineMax = Math.Max(1f, Player.Calamity().adrenalineMax);
            float lostRatio = MathHelper.Clamp(lost / adrenalineMax, 0f, 1f);
            int healAmount = (int)(Player.statLifeMax2 * HealRatioPerFullAdrenaline * lostRatio);

            // 不超过当前缺口，也不溢出最大生命。
            healAmount = Math.Min(healAmount, Player.statLifeMax2 - Player.statLife);
            if (healAmount <= 0)
                return;

            Player.statLife += healAmount;
            Player.HealEffect(healAmount);

            // 调试输出：服务端/单机执行，Main.NewText 会把消息广播到所有客户端聊天栏。
            if (DebugHealMessage)
            {
                string message = Language.GetTextValue(
                    "Mods.Calamitytwinklefragment.Buffs.YanMiaoPower.HealDebug",
                    lost,
                    healAmount,
                    Player.statLife,
                    Player.statLifeMax2);

                Main.NewText(message, new Color(120, 220, 255));
            }
        }
    }
}
