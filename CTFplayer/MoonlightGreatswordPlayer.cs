using System;
using Calamitytwinklefragment.Content.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.CTFplayer
{
    internal class MoonlightGreatswordPlayer : ModPlayer
    {
        /// <summary>剑身是否处于月光附魔状态。</summary>
        public bool MoonlightActive;

        /// <summary>附魔剩余时间（帧）。</summary>
        public int MoonlightTime;

        /// <summary>当前连斩段数（0、1、2）。</summary>
        public int ComboIndex;

        /// <summary>连斩保留时间（帧），超时后重置段数。</summary>
        public int ComboTimer;

        /// <summary>强制隐藏原版手持贴图的剩余时间（帧）。</summary>
        public int HideItemGraphicTimer;

        /// <summary>当前挥砍弹幕的索引，用于在下一刀前移除旧弹幕，避免重复伤害。</summary>
        public int ActiveSwingProjectile = -1;

        public void ActivateMoonlight()
        {
            MoonlightActive = true;
            MoonlightTime = MoonlightGreatsword.MoonlightDuration;
        }

        public override void PostUpdate()
        {
            if (Player.dead)
            {
                ResetState();
                return;
            }

            if (MoonlightTime > 0)
            {
                MoonlightTime--;

                if (MoonlightTime <= 0)
                {
                    MoonlightActive = false;

                    if (Player.whoAmI == Main.myPlayer)
                        SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.45f, Volume = 0.5f }, Player.Center);
                }
            }
            else
            {
                MoonlightActive = false;
            }

            if (ComboTimer > 0)
            {
                ComboTimer--;

                if (ComboTimer <= 0)
                    ComboIndex = 0;
            }

            if (HideItemGraphicTimer > 0)
                HideItemGraphicTimer--;

            if (!MoonlightActive)
                return;

            Lighting.AddLight(Player.Center, 0.35f, 0.55f, 0.95f);

            if (Main.rand.NextBool(4))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(18f, 58f);

                Dust dust = Dust.NewDustPerfect(
                    Player.Center + offset,
                    DustID.Frost,
                    -offset * 0.02f,
                    100,
                    new Color(140, 185, 255),
                    Main.rand.NextFloat(0.9f, 1.35f));
                dust.noGravity = true;
            }
        }

        public override void OnRespawn()
        {
            ResetState();
        }

        private void ResetState()
        {
            MoonlightActive = false;
            MoonlightTime = 0;
            ComboIndex = 0;
            ComboTimer = 0;
            HideItemGraphicTimer = 0;
            ActiveSwingProjectile = -1;
        }
    }
}
