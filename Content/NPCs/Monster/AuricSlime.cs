using CalamityMod.Items.Placeables.Banners;
using CalamityMod.NPCs;
using CalamityMod;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.Utilities;
using Terraria.ModLoader;
using Terraria;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Placeables.Ores;
using Calamitytwinklefragment.Content.Items.Placeables.Banners;

namespace Calamitytwinklefragment.Content.NPCs.Monster
{
    class AuricSlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Slime;
            AIType = NPCID.ToxicSludge;
            NPC.damage = 400;
            NPC.width = 40;
            NPC.height = 30;
            NPC.defense = 30;
            NPC.lifeMax = 100000;
            NPC.knockBackResist = 0f;
            AnimationType = NPCID.CorruptSlime;
            NPC.value = Item.buyPrice(0, 0, 50, 0);
            NPC.alpha = 50;
            NPC.lavaImmune = false;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.Calamity().VulnerableToHeat = true;
            Banner = NPC.type; // 设置旗帜类型
            BannerItem = ModContent.ItemType<AuricSlimeBanner>(); // 设置旗帜物品

            // Scale stats in Expert and Master
            CalamityGlobalNPC.AdjustExpertModeStatScaling(NPC);
            CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
            [
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
                // 描述文本（直接写入字符串）
                new FlavorTextBestiaryInfoElement("史莱姆在吸收了巨龙的力量后进化而成。令人难以置信的是，这种原本简单的生物竟然能够驾驭如此强大的能量。它的身体闪烁着金源的光辉，带电的外壳让任何靠近的敌人都感受到巨龙般的威压。")
            ]);
        }

        public override void AI()
        {
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.PlayerSafe || !DownedBossSystem.downedYharon || spawnInfo.Player.Calamity().ZoneAbyss ||
                spawnInfo.Player.Calamity().ZoneSunkenSea)
            {
                return 0f;
            }
            return SpawnCondition.Cavern.Chance * 0.02f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)55, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)55, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            // 1. 将玩家以极高的速度弹飞
            // 计算弹飞方向（从 NPC 指向玩家）
            Vector2 direction = target.Center - NPC.Center;
            direction.Normalize(); // 归一化方向向量
            float knockbackSpeed = 30f; // 弹飞速度，可以根据需要调整
            target.velocity = direction * knockbackSpeed;

            // 2. 给玩家施加减益
            int buffDuration = 180; // 减益持续时间（单位：帧，60帧 = 1秒）
            target.AddBuff(BuffID.Electrified, buffDuration);
            target.AddBuff(ModContent.BuffType<Dragonfire>(), buffDuration);

            // 3. 播放音效（可选）
            SoundEngine.PlaySound(SoundID.Item94, target.Center); // 使用带电音效
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(ModContent.ItemType<AuricOre>(), 5, 50, 100);
    }
}
