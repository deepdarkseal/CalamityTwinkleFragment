using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using CalamityMod.NPCs;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Materials;
using CalamityMod.Buffs.Summon;
using Microsoft.Xna.Framework;
using Calamitytwinklefragment.Content.Buff.Debuffs;
using Calamitytwinklefragment.Content.Projectiles.NPCsProj.LunaGod;
using Terraria.Audio;
using Terraria.WorldBuilding;
using Nest;
using Calamitytwinklefragment.Content.TFTools;
using CalamityMod.World;
using CalamityMod;

namespace Calamitytwinklefragment.Content.NPCs.BossGod
{
    [AutoloadBossHead]
    class LunaGoddessLunatica : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 200;//这两个代表这个NPC的碰撞箱宽高，以及tr会从你的贴图里扣多大的图
            NPC.damage = 70;
            NPC.lifeMax = Life();
            NPC.defense = 50;
            NPC.scale = 2.0f;//npc的贴图和碰撞箱的放缩倍率
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit5;//挨打时发出的声音
            NPC.DeathSound = SoundID.NPCDeath7;//趋势时发出的声音
            NPC.value = Item.buyPrice(3, 0, 0, 0);//NPC的爆出来的MONEY的数量，四个空从左到右是铂金，金，银，铜
            NPC.lavaImmune = true;//对岩浆免疫
            NPC.noGravity = true;//不受重力影响。一般BOSS都是无重力的
            NPC.noTileCollide = true;//可穿墙
            NPC.npcSlots = 20; //NPC所占用的NPC数量，在TR世界里，NPC上限是200个，通常，这个用来限制Boss战时敌怪数量，填个10，20什么的
            NPC.boss = true; //将npc设为boss 会掉弱治药水和心，会显示xxx已被击败，会有血条
            NPC.dontTakeDamage = false;//为true则为无敌，这里的无敌意思是弹幕不会打到npc，并且npc的血条也不会显示了
            Music = MusicLoader.GetMusicSlot("Calamitytwinklefragment/Assets/Music/BrainDamage");
            NPC.Calamity().canBreakPlayerDefense = true;

            // Scale stats in Expert and Master
            CalamityGlobalNPC.AdjustExpertModeStatScaling(NPC);
            CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC);
        }
        public static int Life()
        {
            if (Main.masterMode) return 350000;
            else if (Main.expertMode && !Main.masterMode) return 295300;
            else return 295300;
        }
        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            Main.npcFrameCount[NPC.type] = 1;
            _ = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = [
                    ModContent.BuffType<Nightwither>(),
                    ModContent.BuffType<CalamityMod.Buffs.StatDebuffs.GlacialState>(),
                    BuffID.Frostburn,
                    BuffID.Frostburn2,
                    ModContent.BuffType<CalamityMod.Buffs.StatDebuffs.GalvanicCorrosion>(),
                    BuffID.Poisoned,
                    BuffID.Venom,
                    BuffID.OnFire,
                    BuffID.OnFire3,
                ]
            };
        }
        // 基础伤害减免（例如30% = 0.3f）
        private const float BaseDamageReduction = 0.3f;
        // 战斗开始时间（基于游戏总帧数）
        private float combatStartTime = 0;
        // 动态伤害减免值
        private float reactiveDamageReduction = 0f;
        // 战斗中是否出现过日食
        private bool hasEclipseOccurred = false;
        //最大生命值
        private float lunamaxlif;
        // 非日食时是否受到伤害
        private bool wasHitOutsideEclipse = false;
        //50%转阶段
        private bool hen50shin = false;
        private bool hen50shining = false;
        // 定义 bossArea 字段
        private Rectangle bossArea;
        //癫狂之拥
        private int Lunatarhug = 0;
        private bool killpplaying = false;
        private int randhugtime;
        //月神之镞
        private bool Arrowheading = false;
        //移动
        private const float WanderDistance = 800f;  // 游荡时与目标点的水平距离
        private const float MaxChaseDistance = 800f; // 触发追击的最大距离
        private const float BaseSpeed = 10f;        // 基础移动速度
        public override void AI()
        {
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];
            // 检测玩家中心坐标周围是否有物块并施加Debuff
            if (NPC.active) // 确保Boss存活
            {

                if (player.active && !player.dead) // 确保玩家存活
                {
                    // 检测玩家中心坐标周围是否有物块
                    if (IsPlayerNearTiles(player))
                    {
                        // 施加LunaLockedDebuff
                        player.AddBuff(ModContent.BuffType<LunaLockedDebuff>(), 120);
                    }
                }
                
            }
            //——动态减伤——
            // 最大生命值
            lunamaxlif = Main.expertMode ? 295300 * 2 : 295300; // 专家和普通
            if (Main.masterMode)
            {
                lunamaxlif = 787500; // 大师模式
            }

            // 检查当前是否为日食
            combatStartTime++;
            if (Main.eclipse)
            {
                hasEclipseOccurred = true; // 标记日食发生过
            }
            float healthRatio = NPC.life / lunamaxlif;
            // 如果日食发生过，则激活动态减伤逻辑
            if (hasEclipseOccurred)
            {
                // 计算血量比率
                float health50Ratio;
                if (healthRatio > 0.5f)
                {
                    // 前 50% 血量
                    health50Ratio = (NPC.life-(lunamaxlif / 2)) / (lunamaxlif / 2);
                }
                else
                {
                    // 后 50% 血量
                    health50Ratio = NPC.life/ (lunamaxlif / 2);
                }

                // 分段计算时间
                float timeRatio;
                if (healthRatio > 0.5f)
                {
                    // 前 50% 血量，持续 2 分半（150 秒）                                       
                    timeRatio = combatStartTime / 9400f;// 计算时间比率
                }
                else
                {
                    // 后 50% 血量，持续 1 分半（90 秒）
                    timeRatio = combatStartTime - 9000f / 5000f;// 计算时间比率
                }

                // 检查是否启用动态伤害减免
                if (timeRatio + health50Ratio < 1f)
                {
                    // 计算动态伤害减免
                    reactiveDamageReduction = 10f - (10f/ (2f - timeRatio - health50Ratio));
                    // 限制动态伤害减免的范围
                    reactiveDamageReduction = MathHelper.Clamp(reactiveDamageReduction, -1f, 10f);
                }
            }
            //——动态减伤——
            //——攻击——
            NPC.ai[0]++;
            if (NPC.ai[0] == 1)//选择
            {
            }
            if (NPC.ai[0] == 53)
            {
                LaunchLunaSilkAttack();
            }
            if (NPC.ai[0] == 106)//攻击
            {
                LaunchTripleWebs();
            }
            if (NPC.ai[0] == 212)
            {
                LaunchOctagonProjectiles();
            }
            if (NPC.ai[0] == 318)//攻击
            {
                LaunchTripleWebs();
            }
            if (NPC.ai[0] == 424)//杀招
            {
                killpplaying = true;
                if (player.HasBuff(ModContent.BuffType<LunaLockedDebuff>()))
                {
                    NPC.ai[1] = Main.rand.Next(0, 4); // 随机选择下次攻击类型（0、1、2 或 3）
                }
                else
                {
                    NPC.ai[1] = Main.rand.Next(0, 3); // 随机选择下次攻击类型（0、1、2）
                }
                Main.NewText($"选择{NPC.ai[1]}", Color.Yellow);
            }
            if (killpplaying)
            {
                ExecuteAttack();
            }
            if (killpplaying && NPC.ai[0]%106==0 && player.velocity.Length() >= 40)
            {
                LaunchLunaSilkAttack();
            }

            //——攻击——
            //——转阶段——
            if (healthRatio <= 0.5f && !hen50shin)
            {
                hen50shining = true;
                hen50shin = true;
                // 播放新的音乐
                Music = MusicLoader.GetMusicSlot("Calamitytwinklefragment/Assets/Music/Eclipse"); // 设置新音乐
                NPC.ai[0] = 319;
                LunaCrazyHug();
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<SilverLunaPlate>(),
                    NPC.damage / 3,
                    0f,
                    Main.myPlayer
                );
            }
            //——转阶段——
            //——移动——
            if (!killpplaying)
            {
                LunaMove();
            }
            //——移动——
            //--- 脱战检测逻辑 ---
            bool shouldDespawn = false;
            // 条件1：非日食的白天（白天且无日食）
            bool isDayWithoutEclipse = Main.dayTime && !Main.eclipse;
            // 条件2：范围内无玩家
            bool noPlayerInRange = true;
            // 更新 bossArea 范围
            UpdateBossArea();
            if (player.active && !player.dead && bossArea.Contains(player.Center.ToPoint()))
            {
                noPlayerInRange = false;
            }
            // 触发脱战条件
            if (isDayWithoutEclipse || noPlayerInRange)
            {
                shouldDespawn = true;
            }
            // 执行脱战
            if (shouldDespawn)
            {
                DespawnBoss();
            }
            //——脱战检测逻辑——
        }
        //**********攻击**********
        private void ExecuteAttack()
        {
            switch (NPC.ai[1])
            {
                case 0:
                    LunaGodnessArrowhead();//月神之镞
                    //Main.NewText($"0.{NPC.ai[1]}", Color.Yellow);
                    break;
                case 1:
                    LunaGodnessArrowhead();
                    //Main.NewText($"0.{NPC.ai[1]}", Color.Yellow);
                    break;
                case 2:
                    LunaCrazyHug();
                    //Main.NewText($"1.{NPC.ai[1]}", Color.Yellow);
                    break;
                case 3:
                    LunaCrazyHug();
                    //Main.NewText($"1.{NPC.ai[1]}", Color.Yellow);
                    break;
            }
        }
        private void LunaCrazyHug()//癫狂之拥
        {
            Player player = Main.player[NPC.target];
            // 瞬移至玩家正上方
            if (Lunatarhug == 0)
            {
                // 在原先位置生成黑色粒子特效
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.ShadowbeamStaff, 0f, 0f, 100, default, 2f);
                }

                // 随机生成一个水平偏移量
                float horizontalOffset = Main.rand.Next(-200, 200);
                if (NPC.life / lunamaxlif >= 0.8f)
                {
                    horizontalOffset = 0;
                }

                // 瞬移至玩家上方的一条水平线上
                NPC.position = new Vector2(player.Center.X - NPC.width / 2 + horizontalOffset, player.Center.Y - NPC.height - 200);

                // 在瞬移后位置生成白色粒子特效
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WhiteTorch, 0f, 0f, 100, default, 2f);
                }

                // 设置伤害
                NPC.damage = Main.expertMode ? 260 : 130; // 专家和普通
                if (Main.masterMode)
                {
                    NPC.damage = 420; // 大师模式
                }
                if (CalamityWorld.death)
                {
                    NPC.damage = (int)(NPC.damage * 1.5f);//死亡模式
                }
                if (hasEclipseOccurred)
                {
                    NPC.damage = NPC.damage * 2;
                }
                //Main.NewText($"1", Color.Yellow);
                // 进入同步阶段
                Lunatarhug = 1;
                randhugtime = Main.rand.Next(30, 45); // 随机同步时间
            }
            if (Lunatarhug == 1)
            {
                NPC.velocity.X = player.velocity.X; // 同步水平速度
                NPC.velocity.Y = player.velocity.Y;
                if (NPC.ai[0] - 424 == randhugtime)
                {
                    // 同步阶段结束，进入下落阶段
                    Lunatarhug = 2;
                    NPC.velocity.Y = 0;
                    NPC.velocity.X = 0; // 重置速度
                    //Main.NewText($"2", Color.Red);
                    float predictionTime = 20;// 预判时间
                    // 预判玩家的位置
                    predictedPlayerPosition = PredictionTF.PredictionPositionTF(player.Center, player.velocity, predictionTime);
                    // 计算从 NPC 当前位置指向预判位置的向量
                    targetVelocity = PredictionTF.YourTargetTF(NPC.Center, predictedPlayerPosition, 3.5f);
                }
            }
            // 下落阶段
            if (Lunatarhug == 2)
            {
                // 直接将 NPC 的速度设置为预判方向的速度
                NPC.velocity += targetVelocity;
                // 计算 NPC 与玩家之间的距离
                float distanceToPlayer = NPC.Distance(player.Center);

                // 如果距离大，减速至停止
                if (distanceToPlayer >= 500)
                {
                    // 减速逻辑
                    NPC.velocity *= 0.9f; // 减速
                    if (NPC.velocity.Length() < 0.1f) // 如果速度接近 0，则完全停止
                    {
                        NPC.velocity = Vector2.Zero;
                    }
                }
            }

            // 结束
            if (NPC.ai[0] >= 530)
            {
                NPC.ai[0] = 0; // 重置计时器
                NPC.damage = 70; // 恢复伤害
                Lunatarhug = 0; // 重置攻击阶段
                NPC.velocity = Vector2.Zero; // 重置速度
                killpplaying = false;
                //Main.NewText($"3", Color.Yellow);
            }
        }
        private void LunaGodnessArrowhead()//月神之镞
        {
            Arrowheading = true;
            // 获取目标玩家
            Player player = Main.player[NPC.target];

            // 如果玩家无效（死亡或不存在），则停止攻击
            if (player.dead || !player.active)
            {
                return;
            }
            NPC.velocity.X = 0; // 重置速度
            NPC.velocity.Y = player.velocity.Y;
            // 定义攻击的总持续时间（帧数）
            int attackDuration = 318; // 持续 5 秒（60 帧 = 1 秒）
            int Arrowheaddamage = Main.expertMode ? 46 : 68; // 专家和普通
            if (Main.masterMode)
            {
                Arrowheaddamage = 42; // 大师模式
            }
            if (CalamityWorld.death)
            {
                Arrowheaddamage = (int)(Arrowheaddamage * 1.5f);//死亡模式
            }
            if (hasEclipseOccurred)
            {
                Arrowheaddamage *= 2;
            }
            // 定义镞弹幕的间隔和随机偏移
            int arrowInterval = 30; // 发射一次镞弹幕
            float arrowSpread = 200f; // 镞弹幕的水平分布范围
            float arrowSpeed = 15f; // 镞弹幕的下落速度
            float fastArrowSpeed = 25f; // 快速镞弹幕的速度
            int type = ModContent.ProjectileType<GodnessArrowhed>();
            // 消除场上的 "MoonShadowWeb" 弹幕
            if ((NPC.ai[0] - 424) == 0) // 仅在攻击开始时执行一次
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile projectile = Main.projectile[i];
                    if (projectile.active && projectile.type == ModContent.ProjectileType<MoonShadowWeb>())
                    {
                        projectile.Kill(); // 消除弹幕
                    }
                    if (projectile.active && projectile.type == ModContent.ProjectileType<LunaDevouringProjectile>())
                    {
                        projectile.Kill(); // 消除弹幕
                    }
                }
            }
            // 每隔 53 帧发射一个快速镞弹幕
            if ((NPC.ai[0] - 424) % 106 == 0)
            {
                Main.NewText($"5", Color.Yellow);
                // 计算快速镞弹幕的发射位置（玩家头顶）
                Vector2 spawnPosition = new(player.Center.X+800, player.Center.Y - 1000);
                Vector2 direction;
                // 检查玩家是否有“锁定”debuff
                // 计算快速镞弹幕的方向
                if (player.HasBuff(ModContent.BuffType<Buff.Debuffs.LunaLockedDebuff>()))//预判攻击
                {  
                    direction = PredictionTF.YourTargetTF(spawnPosition, PredictionTF.PredictionPositionTF(player.Center, player.velocity, 30), 1f);
                }
                else//自机狙
                {
                    direction = PredictionTF.YourTargetTF(spawnPosition, player.Center, 1f);
                }
                // 发射快速镞弹幕
                if (Main.netMode != NetmodeID.MultiplayerClient) // 确保只在服务器端生成弹幕
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition, direction * fastArrowSpeed, type, Arrowheaddamage, 0f, Main.myPlayer);
                }
            }
            // 每隔 arrowInterval 帧发射一组镞弹幕
            if ((NPC.ai[0] - 424) % arrowInterval == 0)
            {
                // 计算镞弹幕的发射位置（玩家头顶）
                Vector2 spawnPosition = new(player.Center.X, player.Center.Y - 1000);

                // 生成一组镞弹幕，形成固定间隔的横排
                int arrowCount = 21; // 每组镞弹幕的数量
                float totalSpread = (arrowCount - 1) * arrowSpread; // 总分布范围
                float startX = spawnPosition.X - totalSpread / 2; // 起始 X 位置

                for (int i = 0; i < arrowCount; i++)
                {
                    
                    // 计算每个镞弹幕的水平偏移
                    float offsetX = startX + i * arrowSpread + Main.rand.NextFloat(-(arrowSpread/10), (arrowSpread / 10));

                    // 计算镞弹幕的发射位置和方向
                    Vector2 arrowPosition = new(offsetX, spawnPosition.Y);
                    Vector2 arrowDirection = Vector2.UnitY; // 垂直向下

                    // 发射镞弹幕
                    if (Main.netMode != NetmodeID.MultiplayerClient) // 确保只在服务器端生成弹幕
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), arrowPosition, arrowDirection * arrowSpeed, type, Arrowheaddamage, 0f, Main.myPlayer);
                    }
                }
            }
            //结束
            if ((NPC.ai[0] - 424) >= attackDuration)
            {
                NPC.ai[0] = 0;
                NPC.velocity = Vector2.Zero; // 重置速度
                killpplaying = false;
                Arrowheading = false;
            }
        }
        // 攻击①：月蚀
        private void LaunchOctagonProjectiles()
        {
            Vector2 center = NPC.Center;
            float radius = 200f; // 半径
            int projectileCount = 8;
            int Octagondamage = Main.expertMode ? 46 : 68; // 专家和普通
            if (Main.masterMode)
            {
                Octagondamage = 42; // 大师模式
            }
            if (CalamityWorld.death)
            {
                Octagondamage = (int)(Octagondamage * 1.5f);//死亡模式
            }
            if (hasEclipseOccurred)
            {
                Octagondamage *= 2;
            }
            for (int i = 0; i < projectileCount; i++)
            {
                // 计算角度（正八边形间隔45度）
                float angle = MathHelper.ToRadians(45 * i);

                // 计算生成位置
                Vector2 spawnPosition = center + new Vector2(
                    radius * (float)Math.Cos(angle),
                    radius * (float)Math.Sin(angle)
                );

                // 计算弹幕朝向（尖端指向中心）
                float rotation = angle + MathHelper.Pi; // 180度转向

                // 生成弹幕（速度设为0，由弹幕自身逻辑控制移动）
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    spawnPosition,
                    Vector2.Zero,
                    ModContent.ProjectileType<LunaDevouringProjectile>(),
                    Octagondamage,
                    0f,
                    Main.myPlayer,
                    rotation
                );
            }

            // 播放音效
            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
        }
        // 攻击②：月影网
        private void LaunchTripleWebs()
        {
            Vector2 center = NPC.Center;
            float speed = 8f; // 弹幕速度
            int projectileCount = 3;
            int Websdamage = Main.expertMode ? 42 : 50; // 专家和普通
            if (Main.masterMode)
            {
                Websdamage = 38; // 大师模式
            }
            if (CalamityWorld.death)
            {
                Websdamage = (int)(Websdamage * 1.5f);//死亡模式
            }
            if (hasEclipseOccurred)
            {
                Websdamage *= 2;
            }
            for (int i = 0; i < projectileCount; i++)
            {
                // 计算角度（120度间隔）
                float angle = MathHelper.ToRadians(120 * i);

                // 计算速度方向
                Vector2 velocity = new(
                    speed * (float)Math.Cos(angle),
                    speed * (float)Math.Sin(angle)
                );

                // 生成弹幕
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    center,
                    velocity,
                    ModContent.ProjectileType<MoonShadowWeb>(),
                    Websdamage,
                    0f,
                    Main.myPlayer
                );
            }

            // 播放音效
            SoundEngine.PlaySound(SoundID.Item17, NPC.Center);
        }
        private void LaunchLunaSilkAttack()
        {
            // 获取目标玩家
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                // 处理玩家无效情况
                return;
            }
            int silketime = Main.expertMode ? 32 : 36; // 专家和普通
            if (Main.masterMode)
            {
                silketime = 28; // 大师模式
            }
            if (CalamityWorld.death)
            {
                silketime = (int)(silketime * 0.8f);//死亡模式
            }
            // 计算预判位置
            Vector2 predictedPos = PredictionTF.PredictionPositionTF(player.Center, player.velocity, silketime);

            // 计算玩家的移动方向
            Vector2 playerDirection = player.velocity.SafeNormalize(Vector2.UnitY); // 获取玩家的移动方向
            if (playerDirection == Vector2.Zero)
            {
                playerDirection = Vector2.UnitY; // 如果玩家没有移动，默认方向为向下
            }

            // 计算垂直于玩家移动方向的方向
            Vector2 perpendicularDirection = new(-playerDirection.Y, playerDirection.X); // 旋转90度

            // 计算弹幕的旋转角度
            _ = perpendicularDirection.ToRotation();

            // 生成弹幕（仅服务器端）
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int Silkdamage = Main.expertMode ? 37 : 51; // 专家和普通
                if (Main.masterMode)
                {
                    Silkdamage = 32; // 大师模式
                }
                if (CalamityWorld.death)
                {
                    Silkdamage = (int)(Silkdamage * 1.5f); // 死亡模式
                }
                if (hasEclipseOccurred)
                {
                    //Silkdamage *= 2;
                }
                int type = ModContent.ProjectileType<LunaSilkProj>();

                // 生成 9 个弹幕，组成一条垂直于玩家前进方向的线
                int numProjectiles = 9; // 弹幕数量
                float spacing = 40f; // 弹幕之间的间距
                float totalLength = (numProjectiles - 1) * spacing; // 整条线的总长度
                Vector2 startPos = predictedPos - perpendicularDirection * (totalLength / 2); // 起始位置

                for (int i = 0; i < numProjectiles; i++)
                {
                    // 计算每个弹幕的位置
                    Vector2 position = startPos + perpendicularDirection * spacing * i;

                    // 生成弹幕
                    _ = Projectile.NewProjectile(
                        NPC.GetSource_FromAI(), // 生成源
                        position,               // 位置
                        Vector2.Zero,           // 速度（根据弹幕调整）
                        type,
                        Silkdamage,
                        0f,
                        Main.myPlayer
                    );
                }
            }
        }
        //**********攻击**********
        //**********移动**********
        private void LunaMove()
        {
            // 获取目标玩家
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                // 处理玩家无效情况
                return;
            }
            // 定义两个目标点（例如玩家左右两侧）
            Vector2 targetLeft = new(player.Center.X - WanderDistance, player.Center.Y);
            Vector2 targetRight = new(player.Center.X + WanderDistance, player.Center.Y);

            // 计算 BOSS 到两个目标点的距离
            float distanceToLeft = Vector2.Distance(NPC.Center, targetLeft);
            float distanceToRight = Vector2.Distance(NPC.Center, targetRight);

            // 选择最近的目标点
            Vector2 targetPos = distanceToLeft < distanceToRight ? targetLeft : targetRight;

            // 计算到目标点的距离
            float distanceToTarget = Vector2.Distance(NPC.Center, targetPos);

            // 判断状态：游荡或追击
            if (distanceToTarget > MaxChaseDistance)
            {
                ChaseTarget(targetPos); // 追击目标点
            }
            else
            {
                WanderAroundTarget(targetPos); // 游荡状态
            }

            // 更新面向方向
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }
        private void ChaseTarget(Vector2 targetPos)
        {
            // 指向目标点
            Vector2 direction = (targetPos - NPC.Center).SafeNormalize(Vector2.Zero);

            // 无上限加速追击
            NPC.velocity += direction * 0.3f;
        }
        private void WanderAroundTarget(Vector2 targetPos)
        {
            // 平滑移动至目标位置
            Vector2 toTarget = targetPos - NPC.Center;
            Vector2 desiredVelocity = toTarget.SafeNormalize(Vector2.Zero) * BaseSpeed;

            // 应用速度（带惯性效果）
            NPC.velocity += (desiredVelocity - NPC.velocity) * 0.1f;

            // 添加随机高度偏移
            float verticalOffset = Main.rand.Next(-50, 50);
            NPC.velocity.Y += verticalOffset * 0.02f;
        }
        //**********移动**********
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            // 如果当前不是日食且受到伤害，则设置 wasHitOutsideEclipse 为 true
            if (!Main.eclipse)
            {
                wasHitOutsideEclipse = true;
            }
            // 应用基础伤害减免
            
            // 如果日食发生过，则应用动态伤害减免
            if (hasEclipseOccurred)
            {
                modifiers.FinalDamage *= (1f - reactiveDamageReduction);
            }
            else
            {
                modifiers.FinalDamage *= (1f - BaseDamageReduction);
            }
            float damageThreshold = lunamaxlif * 0.1f; // 计算阈值

            // 获取最终伤害值（考虑所有减免后的伤害）
            float finalDamage = modifiers.FinalDamage.Flat * modifiers.FinalDamage.Multiplicative;

            // 如果最终伤害大于阈值，则将伤害设置为1
            if (finalDamage > damageThreshold)
            {
                modifiers.SetMaxDamage(1); // 将伤害强制设置为1
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<Nightwither>(), 180);
            target.AddBuff(ModContent.BuffType<LunaLockedDebuff>(), 180);
        }
        private static bool IsPlayerNearTiles(Player player)
        {
            // 获取玩家中心坐标
            Vector2 playerCenter = player.Center;
            // 定义检测范围
            int widthRange = 4; // 宽度范围为3格
            int heightRange = 5; // 高度范围为2格
            int tileX = (int)(playerCenter.X / 16); // 玩家中心的物块X坐标
            int tileY = (int)(playerCenter.Y / 16); // 玩家中心的物块Y坐标
            // 遍历检测范围内的物块
            for (int lunai = -widthRange / 2; lunai <= widthRange / 2; lunai++)
            {
                for (int lunaj = -heightRange / 2; lunaj <= heightRange / 2; lunaj++)
                {
                    // 获取当前物块
                    Tile tile = Framing.GetTileSafely(tileX + lunai, tileY + lunaj);

                    // 如果物块存在且为固体，则返回true
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        return true;
                    }
                }
                // 检测玩家是否使用钩爪
                if (IsPlayerGrapplingTiles(player))
                {
                    return true;
                }
            }
            // 如果没有找到固体物块，则返回false
            return false;
        }
        // 检测玩家是否使用钩爪并勾住物块
        private static bool IsPlayerGrapplingTiles(Player player)
        {
            // 检查玩家是否正在使用钩爪
            if (player.grappling[0] != -1)
            {
                return true;
            }
            return false;
        }
        // 更新 bossArea 范围
        private void UpdateBossArea()
        {
            bossArea = new Rectangle(
                (int)(NPC.Center.X - 5000 * 16 / 2), // 中心点扩散
                (int)(NPC.Center.Y - 5000 * 16 / 2),
                5000 * 16,
                5000 * 16
            );
        }
        private bool hasDespawnMessageBeenShown = false; // 脱战消息是否已显示
        private Vector2 predictedPlayerPosition;
        private Vector2 targetVelocity;

        private void DespawnBoss()
        {
            // 重置战斗状态
            combatStartTime = 0;
            reactiveDamageReduction = 0f;
            hasEclipseOccurred = false;
            wasHitOutsideEclipse = false;
            hen50shin = false;
            // 提示玩家（仅显示一次）
            if (!hasDespawnMessageBeenShown)
            {
                Main.NewText("月女神 露娜蒂卡失去了对你的兴趣", Color.Silver);
                hasDespawnMessageBeenShown = true;
            }
            NPC.active = false;
            NPC.netUpdate = true; // 同步到多人模式
        }
        public override void OnSpawn(IEntitySource source)
        {
            // 重置脱战消息标志位
            hasDespawnMessageBeenShown = false;
        }
        public override void OnKill()
        {
            // 重置战斗状态
            combatStartTime = 0;
            reactiveDamageReduction = 0f;
            hasEclipseOccurred = false;
            wasHitOutsideEclipse = false;
            hen50shin = false;
        }
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ModContent.ItemType<CalamityMod.Items.Potions.SupremeHealingPotion> ();
            name = "月女神 露娜蒂卡";
        }
    }
}
