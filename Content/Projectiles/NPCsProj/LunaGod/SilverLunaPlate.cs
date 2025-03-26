using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using CalamityMod.World;
using CalamityMod;
using Calamitytwinklefragment.Content.NPCs.BossGod;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Graphics.Effects;
using Calamitytwinklefragment.Content.Buff.Debuffs;

namespace Calamitytwinklefragment.Content.Projectiles.NPCsProj.LunaGod
{
    public class SilverLunaPlate : ModProjectile
    {
        internal static readonly float CircularHitboxRadius = 170f;
        private static readonly int MinimumDamagePerFrame = 4;
        private static readonly int MaximumDamagePerFrame = 16;
        private static readonly float AdrenalineLossPerFrame = 0.04f;
        private static readonly float SpeedToForceMaxDamage = 25f;

        private float speedAdd = 0f;
        private float speedLimit = 0f;
        private int time = 0;
        private int sitStill = 90;

        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 320;
            Projectile.height = 320;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 36000;
            Projectile.Opacity = 0f;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;//修改弹幕的最大绘制距离
        }

        public override void AI()
        {
            // 初始化阶段
            if (time == 0)
            {
                Projectile.scale = 0.1f;
                for (int i = 0; i < 2; i++)
                {
                    Particle bloom = new BloomParticle(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Silver, Color.LightBlue, 0.3f), 1.45f, 0, 120, false);
                    GeneralParticleHandler.SpawnParticle(bloom);
                }
                Particle bloom2 = new BloomParticle(Projectile.Center, Vector2.Zero, Color.White, 1.35f, 0, 120, false);
                GeneralParticleHandler.SpawnParticle(bloom2);
            }

            time++;

            // 弹幕的缩放逻辑
            if (Projectile.scale < 1.9f && Projectile.timeLeft > 90)
            {
                if (Projectile.scale < 1.5f)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 dustVel = new Vector2(30, 30).RotatedByRandom(100) * Main.rand.NextFloat(0.05f, 1.2f);
                        Dust spawnDust = Dust.NewDustPerfect(Projectile.Center * (Projectile.scale * 5), DustID.WhiteTorch, dustVel);
                        spawnDust.noGravity = true;
                        spawnDust.scale = Main.rand.NextFloat(1.7f, 2.8f) - Projectile.scale * 1.5f;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 sparkVel = new Vector2(20, 20).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1.1f);
                        GlowOrbParticle orb = new(Projectile.Center + sparkVel * 2 * (Projectile.scale * 5), sparkVel, false, 60, Main.rand.NextFloat(1.55f, 2.75f) - Projectile.scale * 1.5f, Color.Lerp(Color.Silver, Color.LightBlue, 0.5f), true, true);
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }
                Projectile.scale += 0.01f;
            }

            if (!CalamityPlayer.areThereAnyDamnBosses)
            {
                if (Projectile.timeLeft > 90)
                    Projectile.timeLeft = 90;
                Projectile.netUpdate = true;
            }

            int choice = (int)Projectile.ai[1];
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.soundDelay = 1125 - (choice * 225);
                Projectile.localAI[0] += 1f;
                speedLimit = 23;
            }

            if (speedAdd < speedLimit)
                speedAdd += 0.04f;

            float targetDist;
            if (!Main.player[choice].dead && Main.player[choice].active && Main.player[choice] != null)
                targetDist = Vector2.Distance(Main.player[choice].Center, Projectile.Center);
            else
                targetDist = 2000;

            if (Projectile.ai[1] == 0f)
            {
                if (targetDist <= 1400)
                {
                    _ = Utils.GetLerpValue(1400, 700, targetDist);
                }

                Projectile.soundDelay--;
                if (Projectile.soundDelay <= 0 && Projectile.timeLeft >= 90)
                {
                    Projectile.soundDelay = 1;
                }
            }

            // 弹幕的透明度逻辑
            if (Projectile.timeLeft < 90)
            {
                Projectile.Opacity = MathHelper.Clamp(Projectile.timeLeft / 90f, 0f, 1f);
            }
            else
            {
                Projectile.Opacity = MathHelper.Clamp(1f - ((Projectile.timeLeft - 35910) / 90f), 0f, 1f);
            }

            // 弹幕的移动行为
            if (Projectile.scale >= 1.9f)
                sitStill--;
            if (sitStill > 0)
                return;

            bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            Lighting.AddLight(Projectile.Center, 3f * Projectile.Opacity, 0f, 0f);

            float inertia = (revenge ? 5f : 5.5f) + speedAdd;
            float speed = (revenge ? 2.9f : 2.2f) + (speedAdd * 0.25f);
            float minDist = 160f;

            if (NPC.AnyNPCs(ModContent.NPCType<LunaGoddessLunatica>()))
            {
                inertia *= 1.5f;
                speed *= 0.8f;
            }

            int target = (int)Projectile.ai[0];
            if (target >= 0 && Main.player[target].active && !Main.player[target].dead)
            {
                // 计算与目标的距离
                float distanceToTarget = Projectile.Distance(Main.player[target].Center);

                // 如果距离大于2000，则不断加速靠近
                if (distanceToTarget > 1500f)
                {
                    // 计算朝向目标的方向
                    Vector2 moveDirection = Projectile.SafeDirectionTo(Main.player[target].Center, Vector2.UnitY);

                    // 加速逻辑：速度逐渐增加
                    speedAdd += 0.2f; // 每帧增加速度
                    // 更新弹幕速度
                    Projectile.velocity = moveDirection * (speed + speedAdd);
                }
                // 如果距离小于2000，则恢复正常移动逻辑
                else if (distanceToTarget > minDist)
                {
                    // 计算朝向目标的方向
                    Vector2 moveDirection = Projectile.SafeDirectionTo(Main.player[target].Center, Vector2.UnitY);

                    // 使用惯性公式更新弹幕速度
                    Projectile.velocity = (Projectile.velocity * (inertia - 1.5f) + moveDirection * speed) / inertia;
                }
            }
            else
            {
                // 如果目标无效，重新选择最近的玩家
                Projectile.ai[0] = Player.FindClosest(Projectile.Center, 1, 1);
                Projectile.netUpdate = true;
            }

            if (death)
            {
                speedLimit = 15;
                return;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, CircularHitboxRadius * Projectile.scale * Projectile.Opacity, targetHitbox);

        public override bool CanHitPlayer(Player player)
        {
            if (Projectile.Opacity < 1f)
                return false;

            bool cannotBeHurt = player.HasIFrames() || player.creativeGodMode;
            if (cannotBeHurt)
                return true;

            float distSQ = Projectile.DistanceSQ(player.Center);
            float radiusSQ = CircularHitboxRadius * CircularHitboxRadius * Projectile.scale * Projectile.scale;
            float radiusRatio = distSQ / radiusSQ;

            if (Colliding(Projectile.Hitbox, player.Hitbox) == false)
                return false;

            OnHitPlayer_Internal(player);

            float playerSpeed = player.velocity.LengthSquared();
            float speedRatio = playerSpeed / (SpeedToForceMaxDamage * SpeedToForceMaxDamage);

            float damageApplicationRatio = MathHelper.Max(radiusRatio, speedRatio);

            int healthToDrain = (int)MathHelper.Lerp(MaximumDamagePerFrame, MinimumDamagePerFrame, damageApplicationRatio);
            if (healthToDrain < MinimumDamagePerFrame)
                healthToDrain = MinimumDamagePerFrame;

            player.statLife -= healthToDrain;

            GlowOrbParticle orb = new(player.Center, new Vector2(6, 6).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.1f), false, 60, Main.rand.NextFloat(1.55f, 3.75f), Main.rand.NextBool() ? Color.Silver : Color.Lerp(Color.Silver, Color.LightBlue, 0.5f), true, true);
            GeneralParticleHandler.SpawnParticle(orb);
            if (Main.rand.NextBool())
            {
                GlowOrbParticle orb2 = new(player.Center, new Vector2(6, 6).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.1f), false, 60, Main.rand.NextFloat(1.55f, 3.75f), Color.Black, false, true, false);
                GeneralParticleHandler.SpawnParticle(orb2);
            }

            CalamityPlayer modPlayer = player.Calamity();
            if (modPlayer.AdrenalineEnabled)
                modPlayer.adrenaline *= 1f - AdrenalineLossPerFrame;

            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0 || Projectile.Opacity != 1f)
                return;

            OnHitPlayer_Internal(target);
        }

        private static void OnHitPlayer_Internal(Player target)
        {
            target.AddBuff(ModContent.BuffType<Nightwither>(), 300);
            target.AddBuff(ModContent.BuffType<LunaLockedDebuff>(), 300);
            target.AddBuff(BuffID.Webbed, 30);

            if (NPC.AnyNPCs(ModContent.NPCType<LunaGoddessLunatica>()))
            {
                if (Main.npc[NPC.FindFirstNPC(ModContent.NPCType<LunaGoddessLunatica>())].active)
                {
                    for (int l = 0; l < Player.MaxBuffs; l++)
                    {
                        int buffType = target.buffType[l];
                        if (target.buffTime[l] > 0 && CalamityLists.amalgamBuffList.Contains(buffType))
                        {
                            target.DelBuff(l);
                            l--;
                        }
                    }
                }
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
    }
}

