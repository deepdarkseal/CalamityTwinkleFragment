using Microsoft.Xna.Framework;
using Nest;
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
    internal class PutridFluid : ModNPC
    {
        private int bossIndex = -1;
        private Vector2 velocity = Vector2.Zero;

        // 鸟群算法参数 - 调整权重使流体保持适当距离并快速跟随
        private readonly float separationWeight = 2f;    // 适当分离权重
        private readonly float alignmentWeight = 1.4f;   // 对齐权重
        private readonly float cohesionWeight = 3f;    // 降低内聚权重

        private readonly float separationDistance = 60f; // 增加分离距离
        private readonly float maxSpeed = 20f;           // 增加最大速度以便跟上主体
        private readonly float maxForce = 0.8f;            // 增加最大力

        private Vector2 lastPosition;
        private Vector2 returnForce = Vector2.Zero;      // 返回BOSS中心的力

        // 生命恢复相关
        private int healTimer = 0;
        private const int HEAL_INTERVAL = 1; // 每帧恢复
        private const int HEAL_AMOUNT = 50; // 每次恢复生命值

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }

        public static int Life()
        {
            if (Main.masterMode) return 100000 / 3;
            else if (Main.expertMode) return 100000 / 2;
            else return 100000;
        }

        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 40;
            NPC.damage = 40;
            NPC.defense = 20;
            NPC.lifeMax = Life(); // 每个流体生物的生命值
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = false;
            NPC.chaseable = false;
        }

        public override void AI()
        {
            // 生命恢复逻辑
            healTimer++;
            if (healTimer >= HEAL_INTERVAL && NPC.life < NPC.lifeMax)
            {
                healTimer = 0;
                NPC.life += HEAL_AMOUNT;
                if (NPC.life > NPC.lifeMax)
                    NPC.life = NPC.lifeMax;
            }

            if (bossIndex == -1 || !Main.npc[bossIndex].active || Main.npc[bossIndex].type != ModContent.NPCType<LordOfPutrescence>())
            {
                NPC.active = false;
                return;
            }

            NPC.velocity = velocity;
            lastPosition = NPC.Center;
        }

        public void UpdateFlockBehaviorOptimized(List<int> nearbyFluidIndices, Vector2 center)
        {
            Vector2 separation = Vector2.Zero;
            Vector2 alignment = Vector2.Zero;
            Vector2 cohesion = Vector2.Zero;
            int separationCount = 0;
            int alignmentCount = 0;

            int maxChecks = 15;
            int checkedCount = 0;

            // 计算与BOSS中心的距离
            Vector2 toCenter = center - NPC.Center;
            float distanceToCenter = toCenter.Length();

            // 动态返回力 - 根据距离调整强度
            if (distanceToCenter > 300f) // 如果距离BOSS超过300像素
            {
                float forceStrength = MathHelper.Clamp((distanceToCenter - 300f) / 200f, 0.05f, 0.4f);
                returnForce = toCenter * forceStrength;
            }
            else if (distanceToCenter < 100f) // 如果太靠近BOSS，轻微推开
            {
                returnForce = -toCenter * 0.02f;
            }
            else
            {
                returnForce = Vector2.Zero;
            }

            foreach (int otherIndex in nearbyFluidIndices)
            {
                if (checkedCount >= maxChecks) break;

                if (otherIndex != NPC.whoAmI && otherIndex < Main.maxNPCs && Main.npc[otherIndex].active)
                {
                    NPC other = Main.npc[otherIndex];
                    Vector2 diff = NPC.Center - other.Center;
                    float distance = diff.Length();

                    // 分离 - 当距离过近时推开
                    if (distance > 0 && distance < separationDistance)
                    {
                        diff.Normalize();
                        diff /= distance; // 距离越近，分离力越大
                        separation += diff;
                        separationCount++;
                    }

                    // 对齐 - 与附近流体保持相似运动方向
                    if (distance < 120f)
                    {
                        alignment += other.velocity;
                        alignmentCount++;
                        checkedCount++;
                    }
                }
            }

            // 应用分离力
            if (separationCount > 0)
            {
                separation /= separationCount;
                separation = SetMagnitude(separation, maxSpeed);
                separation -= velocity;
                separation = Limit(separation, maxForce);
            }

            // 应用对齐力
            if (alignmentCount > 0)
            {
                alignment /= alignmentCount;
                alignment = SetMagnitude(alignment, maxSpeed);
                alignment -= velocity;
                alignment = Limit(alignment, maxForce);
            }

            // 内聚（朝向中心）- 调整为保持适当距离
            Vector2 desired = center - NPC.Center;
            if (desired.Length() > 0)
            {
                desired.Normalize();

                // 动态调整内聚力，使流体保持在150-300像素的理想距离
                float idealDistance = 200f;
                float distanceFactor = Math.Abs(distanceToCenter - idealDistance) / idealDistance;
                float cohesionStrength = MathHelper.Clamp(distanceFactor, 0.5f, 1.5f);

                desired *= maxSpeed * cohesionStrength;
                cohesion = desired - velocity;
                cohesion = Limit(cohesion, maxForce * 1.2f);
            }

            // 应用所有力，包括返回力
            velocity += separation * separationWeight;
            velocity += alignment * alignmentWeight;
            velocity += cohesion * cohesionWeight;
            velocity += returnForce;

            // 在理想距离范围内添加环绕运动
            if (distanceToCenter > 150f && distanceToCenter < 350f)
            {
                // 计算切向速度（垂直于半径方向）
                Vector2 tangent = new(-toCenter.Y, toCenter.X);
                tangent.Normalize();

                // 根据距离调整环绕速度
                float orbitSpeed = 0.4f * (1f - Math.Abs(distanceToCenter - 250f) / 100f);
                velocity += tangent * orbitSpeed;
            }

            // 添加轻微随机运动，增加自然感
            if (Main.rand.NextBool(40))
            {
                velocity += new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.1f, 0.1f));
            }

            // 限制最大速度
            velocity = Limit(velocity, maxSpeed);

            // 平滑移动
            if (lastPosition != Vector2.Zero)
            {
                NPC.Center = Vector2.Lerp(lastPosition, NPC.Center + velocity, 0.8f);
            }
        }

        private static Vector2 SetMagnitude(Vector2 vector, float magnitude)
        {
            float length = vector.Length();
            if (length > 0)
            {
                return vector * (magnitude / length);
            }
            return vector;
        }

        private static Vector2 Limit(Vector2 vector, float max)
        {
            float lengthSquared = vector.LengthSquared();
            if (lengthSquared > max * max)
            {
                vector.Normalize();
                return vector * max;
            }
            return vector;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC parentNPC)
            {
                bossIndex = parentNPC.whoAmI;
            }

            // 初始速度朝向BOSS中心
            if (bossIndex != -1 && Main.npc[bossIndex].active)
            {
                Vector2 toBoss = Main.npc[bossIndex].Center - NPC.Center;
                if (toBoss.Length() > 0)
                {
                    toBoss.Normalize();
                    velocity = toBoss * 6f; // 增加初始速度
                }
            }
        }

        // 添加受伤时的血量同步
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            SyncBossHealth();
        }

        private void SyncBossHealth()
        {
            // 当流体受伤时，立即同步BOSS血量
            if (bossIndex != -1 && Main.npc[bossIndex].active && Main.npc[bossIndex].ModNPC is LordOfPutrescence boss)
            {
                boss.ForceUpdateHealth();
            }
        }
        // 新增：修改受到的伤害，根据弹幕穿透属性调整伤害
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            // 这里我们使用HitModifiers来调整伤害
            // 由于我们无法直接获取造成伤害的弹幕，我们需要在OnHitByProjectile中处理
            // 但ModifyIncomingHit可以用于其他类型的伤害调整
        }

        // 重写OnHitByProjectile来处理弹幕伤害调整
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            // 检查弹幕的穿透属性
            if (projectile.penetrate > 1 || projectile.penetrate == -1)
            {
                // 穿透弹幕：减少50%伤害
                NPC.life += (int)(hit.Damage * 0.5f);
                if (NPC.life > NPC.lifeMax)
                    NPC.life = NPC.lifeMax;
            }
            else
            {
                // 非穿透弹幕：增加50%伤害（实际减少生命值恢复）
                int extraDamage = (int)(hit.Damage * 0.5f);
                NPC.life -= extraDamage;
                if (NPC.life < 0)
                    NPC.life = 0;
            }

            SyncBossHealth();
        }
    }
}