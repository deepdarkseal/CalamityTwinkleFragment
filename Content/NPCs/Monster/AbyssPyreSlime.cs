using CalamityMod.NPCs;
using CalamityMod;
using System;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.BiomeManagers;
using Terraria.ModLoader.Utilities;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework.Graphics;

namespace Calamitytwinklefragment.Content.NPCs.Monster
{
    class AbyssPyreSlime : ModNPC
    {
        public override void SetDefaults()
        {
            AIType = NPCID.ToxicSludge;
            NPC.damage = 100;
            NPC.width = 58;
            NPC.height = 40;
            NPC.defense = 50;
            NPC.lifeMax = 5000;
            NPC.knockBackResist = 0f;
            AnimationType = NPCID.CorruptSlime;
            NPC.value = Item.buyPrice(0, 0, 10, 0);
            NPC.alpha = 50;
            NPC.lavaImmune = false;
            NPC.aiStyle = -1; // 自定义 AI
            AIType = -1;
            NPC.chaseable = false;
            NPC.noGravity = true; // 无重力（在水中悬浮）
            NPC.noTileCollide = false; // 与地形碰撞
            NPC.buffImmune[BuffID.OnFire] = true; // 免疫火焰减益
            NPC.buffImmune[BuffID.Poisoned] = true; // 免疫中毒减益
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.Calamity().VulnerableToHeat = true;
            // Banner = NPC.type; // 设置旗帜类型
            //BannerItem = ModContent.ItemType<AuricSlimeBanner>(); // 设置旗帜物品
            SpawnModBiomes = [ModContent.GetInstance<AbyssLayer3Biome>().Type];
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
                new FlavorTextBestiaryInfoElement("史莱姆那强大适应力的又一个例子！通过与硫氧化细菌共生来从热泉中的高温热液获取能量，它们居然适应了这个史莱姆的禁地！")
            ]);
        }

        public override void AI()
        {
            // 基础目标锁定（参考ColossalSquid的玩家搜索逻辑）
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];
            bool playerActive = player.active && !player.dead;
            float distanceToPlayer = Vector2.Distance(NPC.Center, player.Center);

            //------------------------------
            // 状态0：停滞在物块上
            //------------------------------
            if (NPC.ai[0] == 0f)
            {
                // 粘附在物块上
                NPC.velocity *= 0.95f;
                NPC.noTileCollide = false;

                // 检测玩家是否进入警戒范围（15格）
                if (playerActive && distanceToPlayer < 30 * 30)
                {
                    NPC.ai[0] = 1f; // 切换到攻击状态
                    NPC.ai[1] = 0f; // 重置攻击计时器
                    NPC.netUpdate = true;
                }
            }

            //------------------------------
            // 状态1：发射废弃物弹幕
            //------------------------------
            else if (NPC.ai[0] == 1f)
            {
                // 每2秒发射一次（120帧=2秒）
                NPC.ai[1]++;
                if (NPC.ai[1] >= 120)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 projectileVel = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 30f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVel,
                            ModContent.ProjectileType<ToxicMinnowCloud>(), 100, 0f, Main.myPlayer);
                    }
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                }

                // 玩家过近或血量低于30%时切换到逃离状态
                if (distanceToPlayer < 5 * 16 || NPC.life < NPC.lifeMax * 0.5f)
                {
                    NPC.ai[0] = 2f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                }

                // 玩家超出范围返回停滞状态
                if (distanceToPlayer > 30 * 30)
                {
                    NPC.ai[0] = 0f;
                    NPC.netUpdate = true;
                }
            }

            //------------------------------
            // 状态2：沿物块滚动逃离
            //------------------------------
            else if (NPC.ai[0] == 2f)
            {
                // 优先向下移动
                if (NPC.collideY && NPC.velocity.Y == 0)
                {
                    NPC.velocity.Y = -3f; // 反冲跳起
                }
                else
                {
                    NPC.velocity.Y += 0.5f; // 模拟重力
                }

                // 水平移动方向选择
                float pushDirection = (NPC.Center.X < player.Center.X) ? -1 : 1;
                NPC.velocity.X = pushDirection * 4f;

                // 检测前方是否有障碍物
                int tileX = (int)(NPC.Center.X + (pushDirection * NPC.width / 2)) / 16;
                int tileY = (int)(NPC.Center.Y) / 16;
                Tile tile = Framing.GetTileSafely(tileX, tileY);

                // 如果有障碍物，尝试向上跳跃
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    NPC.velocity.Y = -6f; // 向上跳跃
                }

                // 碰撞反弹逻辑（参考ColossalSquid）
                if (NPC.collideX)
                {
                    NPC.velocity.X *= -0.8f;
                    NPC.direction *= -1;
                }

                // 血量恢复后返回攻击状态
                if (NPC.life > NPC.lifeMax * 0.5f && distanceToPlayer > 8 * 16)
                {
                    NPC.ai[0] = 1f;
                    NPC.netUpdate = true;
                }
            }

            // 通用逻辑：限制水中速度
            if (NPC.wet)
            {
                NPC.velocity *= 0.98f; // 模拟水中阻力
                if (NPC.velocity.Length() > 6f) // 检查速度是否超过限制
                {
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitX) * 6f; // 限制速度
                }
            }
            // 缓慢回血
            if (NPC.life < NPC.lifeMax && NPC.life > 0)
            {
                NPC.life += 10; // 每帧恢复的生命值
                if (Main.rand.NextBool(10)) // 10%概率生成治疗粒子效果
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.HealingPlus, 0f, 0f, 0, default, 1f);
                }
            }
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if ((spawnInfo.Player.Calamity().ZoneAbyssLayer3) && spawnInfo.Water)
            {
                return Main.remixWorld ? 5.4f : SpawnCondition.CaveJellyfish.Chance * 0.6f;
            }
            else
            {
                return 0f;
            }            
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemID.Gel, 1, 30, 50);

            var postLevi = npcLoot.DefineConditionalDropSet(DropHelper.PostLevi());
            postLevi.Add(DropHelper.NormalVsExpertQuantity(ModContent.ItemType<DepthCells>(), 2, 6, 8, 11, 15));
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2; // 设置动画帧数为6
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.15f; // 控制帧切换速度
            NPC.frameCounter %= Main.npcFrameCount[NPC.type]; // 确保帧数在有效范围内
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight; // 设置当前帧
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // 如果需要自定义绘制逻辑，可以在这里实现
            return true; // 返回true表示使用默认绘制逻辑
        }

        public override Color? GetAlpha(Color drawColor)
        {
            // 动态光效：使用正弦函数模拟脉动效果
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.9f; // 脉动范围：0.8 ~ 1.0

            // 设置基础红光颜色 (R: 1.0, G: 0.2, B: 0.2)
            Color baseColor = new(1.0f, 0.2f, 0.2f); // 基础红色

            // 动态颜色变化：在红色和橙色之间渐变
            float colorInterpolant = (float)Math.Abs(Math.Sin(Main.GameUpdateCount * 0.03f)); // 颜色插值因子
            Color dynamicColor = Color.Lerp(baseColor, new Color(1.0f, 0.5f, 0.2f), colorInterpolant); // 从红色渐变到橙色

            // 应用脉动效果
            Color finalColor = dynamicColor * pulse;

            // 调整光照强度（根据需要调整）
            float intensity = 1f; // 光照强度
            finalColor *= intensity;

            // 返回最终颜色
            return finalColor;
        }

        public override void PostAI()
        {
            // 生成红色粒子效果
            if (Main.rand.NextBool(10)) // 10%概率生成粒子
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.LifeDrain, 0f, 0f, 0, default, 1f);
                dust.noGravity = true; // 粒子不受重力影响
                dust.velocity *= 0.5f; // 粒子速度
                dust.scale = 1.2f; // 粒子大小
                dust.color = new Color(255, 100, 100); // 粒子颜色（红色）
            }
        }
    }
}
