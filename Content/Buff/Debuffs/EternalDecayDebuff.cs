using CalamityMod;
using CalamityMod.Particles;
using Calamitytwinklefragment.CTFplayer;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Buff.Debuffs
{
    internal class EternalDecayDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true; // 护士无法移除
            BuffID.Sets.LongerExpertDebuff[Type] = true; // 专家模式持续时间更长
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 伤害减少20%
            player.GetDamage(DamageClass.Generic) *= 0.8f;
            player.GetModPlayer<CTFDebuffPlayer>().EternalDecayP = true;
            // 获取玩家当前的生命再生值
            //float currentLifeRegen = player.lifeRegen;
            //player.lifeRegen = 0;
            //int tickDecay = (int)(120 / currentLifeRegen);
            // 造成伤害
            //if (Main.GameUpdateCount % tickDecay == 0)
            {
                //player.statLife -= 4;
            }
            // 绿色霉菌粒子效果
            Color moldColor = Main.rand.Next(3) switch
            {
                0 => Color.DarkGreen,
                1 => Color.ForestGreen,
                _ => Color.LimeGreen,
            };

            // 生成霉菌灰尘粒子
            if (Main.rand.NextBool(5))
            {
                Vector2 dustPosition = player.Center + new Vector2(
                    Main.rand.NextFloat(-player.width / 2, player.width / 2),
                    Main.rand.NextFloat(-player.height / 2, player.height / 2)
                );

                Dust dust = Dust.NewDustPerfect(dustPosition, DustID.Terra, CalamityUtils.RandomVelocity(50f, 30f, 100f, 0.02f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.color = moldColor;
                dust.alpha = 100;
            }

            // 生成绿色霉菌孢子粒子
            if (Main.rand.NextBool(3))
            {
                Vector2 sporePosition = player.Center + new Vector2(
                    Main.rand.NextFloat(-player.width / 2, player.width / 2),
                    Main.rand.NextFloat(-player.height / 2, player.height / 2)
                );

                Particle moldSpore = new GlowSparkParticle(
                    sporePosition,
                    CalamityUtils.RandomVelocity(40f, 20f, 80f, 0.01f),
                    false,
                    6,
                    0.005f,
                    moldColor,
                    new Vector2(0.4f, 0.8f)
                );
                GeneralParticleHandler.SpawnParticle(moldSpore);
            }
        }

        internal static void DrawEffects(NPC npc)
        {
            // 对NPC的霉菌粒子效果
            Vector2 npcSize = npc.Center + new Vector2(
                Main.rand.NextFloat(-npc.width / 2, npc.width / 2),
                Main.rand.NextFloat(-npc.height / 2, npc.height / 2)
            );

            Color moldColor = Main.rand.Next(3) switch
            {
                0 => Color.DarkGreen,
                1 => Color.ForestGreen,
                _ => Color.LimeGreen,
            };

            // 生成霉菌灰尘粒子
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustPerfect(npcSize, DustID.Terra, CalamityUtils.RandomVelocity(50f, 30f, 100f, 0.02f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f) + (0.0000005f * npc.width * npc.height);
                dust.color = moldColor;
                dust.alpha = 100;
            }

            // 生成绿色霉菌孢子粒子
            if (Main.rand.NextBool(6))
            {
                Vector2 sporePosition = npc.Center + new Vector2(
                    Main.rand.NextFloat(-npc.width / 2, npc.width / 2),
                    Main.rand.NextFloat(-npc.height / 2, npc.height / 2)
                );

                Particle moldSpore = new GlowSparkParticle(
                    sporePosition,
                    CalamityUtils.RandomVelocity(40f, 20f, 80f, 0.01f),
                    false,
                    6,
                    0.005f,
                    moldColor,
                    new Vector2(0.4f, 0.8f)
                );
                GeneralParticleHandler.SpawnParticle(moldSpore);
            }
        }
    }
}