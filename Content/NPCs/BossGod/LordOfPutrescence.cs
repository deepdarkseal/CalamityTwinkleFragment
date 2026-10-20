using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.NPCs.BossGod
{
    [AutoloadBossHead]
    public class LordOfPutrescence : ModNPC
    {
        public List<int> fluidMinions = [];

        // 分组更新优化：将流体分成7组，每组11个
        private readonly List<int>[] fluidGroups = new List<int>[7];
        private int currentGroupIndex = 0;
        private const int GROUPS_COUNT = 7;
        private const int FLUIDS_PER_GROUP = 11;
        private const int TOTAL_FLUIDS = 77;

        // 性能优化：使用固定数组存储活跃的流体生物索引
        private readonly int[] activeFluidIndices = new int[TOTAL_FLUIDS];
        private int activeFluidCount = 0;

        // 网格空间分区优化
        private FluidGrid fluidGrid;
        private const int GRID_CELL_SIZE = 200;

        // 初始化标志
        private bool fluidsInitialized = false;

        public enum AttackState
        {
            Normal,
            SpawnFluids,
            SpecialAttack
        }

        public AttackState CurrentState = AttackState.Normal;

        public int spawnTimer = 0;
        public int stateTimer = 0;
        public int aiTimer = 0;
        public int groupUpdateTimer = 0;

        public float spawnSpeedMultiplier = 1.0f;
        public int debuffCount = 0;

        // 生命值设置
        public int totalLife = 0;
        public int totalLifeMax = 7777777; // 77 * 100000 + 77777

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }
        public static int Life()
        {
            if (Main.masterMode) return 77777 / 3;
            else if (Main.expertMode) return 77777 / 2;
            else return 77777;
        }
        public override void SetDefaults()
        {
            NPC.width = 120;
            NPC.height = 120;
            NPC.damage = 100;
            NPC.defense = 50;
            NPC.lifeMax = Life(); // 主体基础生命值
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.netAlways = true;
            NPC.value = Item.buyPrice(5, 0, 0, 0);
            NPC.SpawnWithHigherTime(30);
            NPC.npcSlots = 20f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 初始化空间分区网格
            fluidGrid = new FluidGrid(GRID_CELL_SIZE);

            // 初始化分组
            for (int i = 0; i < GROUPS_COUNT; i++)
            {
                fluidGroups[i] = [];
            }

            // 立即生成所有流体
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                InitializeAllFluids();
            }
        }

        private void InitializeAllFluids()
        {
            for (int i = 0; i < TOTAL_FLUIDS; i++)
            {
                SpawnFluidMinion();
            }
            fluidsInitialized = true;
        }

        public override void AI()
        {
            aiTimer++;
            groupUpdateTimer++;

            UpdateActiveFluidsList();

            // 如果流体未初始化且是服务器，则初始化流体
            if (!fluidsInitialized && Main.netMode != NetmodeID.MultiplayerClient)
            {
                InitializeAllFluids();
            }

            // 只有在流体初始化完成后才执行完整的AI
            if (fluidsInitialized)
            {
                UpdateFluidGroups();
                UpdateAIState();

                switch (CurrentState)
                {
                    case AttackState.Normal:
                        NormalAI();
                        break;
                    case AttackState.SpawnFluids:
                        SpawnFluidsAI();
                        break;
                }

                UpdateFluidGrid();
                UpdateCurrentFluidGroup();
                UpdateOtherFluidGroups();
            }

            // 每帧都更新血量，确保实时同步
            UpdateTotalHealth();

            if (Main.netMode != NetmodeID.MultiplayerClient && aiTimer % 60 == 0)
            {
                NPC.netUpdate = true;
            }
        }

        private void UpdateActiveFluidsList()
        {
            activeFluidCount = 0;

            // 清理无效的流体索引
            for (int i = fluidMinions.Count - 1; i >= 0; i--)
            {
                int index = fluidMinions[i];
                if (index < 0 || index >= Main.maxNPCs || !Main.npc[index].active || Main.npc[index].ModNPC is not PutridFluid)
                {
                    fluidMinions.RemoveAt(i);
                }
            }

            for (int i = 0; i < fluidMinions.Count; i++)
            {
                int index = fluidMinions[i];
                if (index >= 0 && index < Main.maxNPCs && Main.npc[index].active &&
                    Main.npc[index].ModNPC is PutridFluid)
                {
                    if (activeFluidCount < activeFluidIndices.Length)
                    {
                        activeFluidIndices[activeFluidCount] = index;
                        activeFluidCount++;
                    }
                }
            }
        }

        private void UpdateFluidGroups()
        {
            for (int i = 0; i < GROUPS_COUNT; i++)
            {
                fluidGroups[i].Clear();
            }

            for (int i = 0; i < activeFluidCount; i++)
            {
                int groupIndex = i % GROUPS_COUNT;
                fluidGroups[groupIndex].Add(activeFluidIndices[i]);
            }
        }

        private void UpdateFluidGrid()
        {
            fluidGrid.Clear();
            for (int i = 0; i < activeFluidCount; i++)
            {
                NPC fluid = Main.npc[activeFluidIndices[i]];
                fluidGrid.Add(fluid.Center, activeFluidIndices[i]);
            }
        }

        private void UpdateCurrentFluidGroup()
        {
            currentGroupIndex = groupUpdateTimer % GROUPS_COUNT;
            var currentGroup = fluidGroups[currentGroupIndex];

            for (int i = 0; i < currentGroup.Count; i++)
            {
                int fluidIndex = currentGroup[i];
                if (Main.npc[fluidIndex].ModNPC is PutridFluid fluid)
                {
                    var nearbyFluids = fluidGrid.GetNearby(fluid.NPC.Center, 150f);
                    fluid.UpdateFlockBehaviorOptimized(nearbyFluids, NPC.Center);
                }
            }
        }

        private void UpdateOtherFluidGroups()
        {
            for (int groupIdx = 0; groupIdx < GROUPS_COUNT; groupIdx++)
            {
                if (groupIdx == currentGroupIndex) continue;

                var group = fluidGroups[groupIdx];
                for (int i = 0; i < group.Count; i++)
                {
                    int fluidIndex = group[i];
                    NPC fluid = Main.npc[fluidIndex];

                    Vector2 toBoss = NPC.Center - fluid.Center;
                    float distance = toBoss.Length();

                    if (distance > 500f) // 减小触发距离，使流体更靠近
                    {
                        toBoss.Normalize();
                        // 增加向BOSS靠拢的速度和强度
                        fluid.velocity = Vector2.Lerp(fluid.velocity, toBoss * 8f, 0.2f);
                    }
                    else if (distance < 10f) // 如果太靠近，稍微推开
                    {
                        toBoss.Normalize();
                        fluid.velocity = Vector2.Lerp(fluid.velocity, -toBoss * 2f, 0.2f);
                    }
                    else
                    {
                        // 在理想距离内，保持轻微运动
                        fluid.velocity *= 0.95f;
                    }

                    if (fluid.velocity.Length() > 8f)
                    {
                        fluid.velocity.Normalize();
                        fluid.velocity *= 8f;
                    }
                }
            }
        }

        private void UpdateAIState()
        {
            stateTimer++;

            if (stateTimer >= 120)
            {
                stateTimer = 0;
                CurrentState = CurrentState == AttackState.Normal ?
                    AttackState.SpawnFluids : AttackState.Normal;
            }
        }

        private void NormalAI()
        {
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.TargetClosest();
                target = Main.player[NPC.target];
                if (!target.active || target.dead)
                {
                    NPC.velocity = new Vector2(0f, -10f);
                    return;
                }
            }

            Vector2 desiredPosition = GetDesiredPosition(target);
            MoveToPosition(desiredPosition);

            // 只有在流体数量不足时才生成新的
            if (activeFluidCount < TOTAL_FLUIDS)
            {
                spawnTimer++;
                if (spawnTimer >= 300 / spawnSpeedMultiplier)
                {
                    spawnTimer = 0;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SpawnFluidMinion();
                    }
                }
            }
        }

        private Vector2 GetDesiredPosition(Player target)
        {
            float distance = 400f;
            float angleRange = MathHelper.ToRadians(150f);

            float baseAngle = (float)((aiTimer * 0.01f) % (MathHelper.TwoPi)) - angleRange / 2;
            Vector2 offset = new Vector2(0, -distance).RotatedBy(baseAngle);

            return target.Center + offset;
        }

        private void MoveToPosition(Vector2 position)
        {
            Vector2 direction = position - NPC.Center;
            float distance = direction.Length();

            if (distance > 10f)
            {
                direction.Normalize();
                NPC.velocity = direction * MathHelper.Min(distance * 0.1f, 8f);
            }
            else
            {
                NPC.velocity *= 0.95f;
            }
        }

        private void SpawnFluidMinion()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // 严格限制流体数量上限
            if (activeFluidCount >= TOTAL_FLUIDS)
                return;

            Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.Next(-200, 201), Main.rand.Next(-200, 201));
            int minionIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y,
                                       ModContent.NPCType<PutridFluid>(), 0, NPC.whoAmI);

            fluidMinions.Add(minionIndex);
        }

        private void SpawnFluidsAI()
        {
            // 加速生成流体生物，但仍然受数量限制
            if (activeFluidCount < TOTAL_FLUIDS)
            {
                spawnTimer++;
                if (spawnTimer >= 60 / spawnSpeedMultiplier)
                {
                    spawnTimer = 0;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SpawnFluidMinion();
                    }
                }
            }

            Player target = Main.player[NPC.target];
            if (target.active && !target.dead)
            {
                Vector2 desiredPosition = GetDesiredPosition(target);
                MoveToPosition(desiredPosition);
            }
        }

        private void UpdateTotalHealth()
        {
            // 重置总生命值计算
            totalLife = 77777; // 本体当前生命值
            totalLifeMax = 7777777; // 固定最大生命值

            // 累加所有流体生物的生命值
            for (int i = 0; i < activeFluidCount; i++)
            {
                NPC fluid = Main.npc[activeFluidIndices[i]];
                totalLife += fluid.life;
            }

            // 确保总生命值不超过最大值且不为负
            //totalLife = Math.Max(0, Math.Min(totalLife, totalLifeMax));

            // 更新NPC的生命值显示（用于原版血条）
            NPC.life = totalLife;
            NPC.lifeMax = totalLifeMax;

            // 重要：当总生命值归零时，触发BOSS死亡
            if (totalLife <= 77777 && NPC.active)
            {
                NPC.life = 0;
                NPC.checkDead();
            }
        }

        // 添加一个公共方法供流体调用
        public void ForceUpdateHealth()
        {
            UpdateTotalHealth();
        }

        // 修正参数类型
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // 添加战利品
        }
    }
}