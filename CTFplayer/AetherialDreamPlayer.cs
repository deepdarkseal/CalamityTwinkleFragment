// AetherialDreamPlayer.cs
using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Accessories;
using Calamitytwinklefragment.Content.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization; // 添加这个命名空间以使用NetworkText
using Terraria.ModLoader;
using static CalamityMod.CalPlayer.CalamityPlayer;

namespace Calamitytwinklefragment.CTFplayer
{
    internal class AetherialDreamPlayer : ModPlayer
    {
        public bool hasAetherialDream = false;
        public int cooldownTimer = 0;
        public int originalDamage = 0;
        public const int Cooldown = 30 * 60; //冷却(以帧为单位)

        // 用于冷却完成的粒子效果
        private bool showCooledDownEffect = false;
        private int effectTimer = 0;

        public override void ResetEffects()
        {
            hasAetherialDream = false;
        }

        public override void PostUpdate()
        {
            // 冷却计时器
            if (cooldownTimer > 0)
            {
                cooldownTimer--;

                // 检查冷却是否刚刚完成
                if (cooldownTimer == 0)
                {
                    showCooledDownEffect = true;
                    effectTimer = 30; // 效果持续30帧
                }
            }

            // 处理冷却完成的效果
            if (showCooledDownEffect)
            {
                effectTimer--;

                // 生成白色粒子效果
                if (effectTimer % 5 == 0) // 每5帧生成一次粒子
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 velocity = new(
                            Main.rand.NextFloat(-2f, 2f),
                            Main.rand.NextFloat(-3f, -1f)
                        );

                        Dust dust = Dust.NewDustDirect(
                            Player.position,
                            Player.width,
                            Player.height,
                            DustID.WhiteTorch,
                            velocity.X,
                            velocity.Y,
                            100,
                            default,
                            1.5f
                        );
                        dust.noGravity = true;
                    }
                }

                // 效果结束
                if (effectTimer <= 0)
                {
                    showCooledDownEffect = false;
                }
            }
        }

        // 使用ModifyHurt方法处理伤害
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (hasAetherialDream && cooldownTimer <= 0 && !Player.dead)
            {
                // 激活冷却
                cooldownTimer = Cooldown;
                // 将本次伤害变为1
                modifiers.SetMaxDamage(1);
                CalamityPlayer calPlayer = Player.GetModPlayer<CalamityPlayer>();
                Item accessory = calPlayer.FindAccessory(ModContent.ItemType<Content.Items.Accessories.Wings.AetherialDream>());
                IEntitySource source = Player.GetSource_Accessory(accessory);
                Vector2 position = Player.Center;
                for (int i = 0; i < 20; i++)
                {
                    SoundEngine.PlaySound(SoundID.Item61, Player.Center);
                    _ = Projectile.NewProjectile(source, Player.Center.X, Player.Center.Y, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-0f, -4f), ModContent.ProjectileType<AetherialDreamFeather>(), 0, 0, Player.whoAmI);
                }
            }
        }
        // 添加冷却时间显示（可选）
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (hasAetherialDream && cooldownTimer > 0)
            {
                // 冷却期间显示特殊效果
                float progress = (float)cooldownTimer / Cooldown;
                r = 1f;
                g = 1f - progress;
                b = 1f - progress;
            }
        }
    }
}