using CalamityMod.Projectiles;
using Calamitytwinklefragment.Content.Buff.Summom;
using Calamitytwinklefragment.Content.TFTools;
using Calamitytwinklefragment.CTFplayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Projectiles.Summom
{
    internal class GreatTitMinion : ModProjectile
    {
        private bool isNesting = false;
        private int nestTimer = 0;
        private const int MaxNestDistance = 100; // 归巢时的最大距离

        public Player Owner => Main.player[Projectile.owner];

        public ref float CheckForSpawning => ref Projectile.localAI[0];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 22;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25;
        }

        public override bool? CanCutTiles()
        {
            return !isNesting;
        }

        public override bool MinionContactDamage()
        {
            return !isNesting;
        }

        public override void AI()
        {
            // 检查玩家是否还活着并且有buff
            if (Owner.dead || !Owner.active)
            {
                Owner.ClearBuff(ModContent.BuffType<GreatTitBuff>());
                return;
            }

            if (Owner.HasBuff(ModContent.BuffType<GreatTitBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            // 生成特效（只执行一次）
            if (CheckForSpawning == 0f)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi / 20f * i;
                    Vector2 direction = angle.ToRotationVector2() * 5f;

                    Dust spawnDust = Dust.NewDustPerfect(Projectile.Center, DustID.Cloud, direction);
                    spawnDust.noGravity = true;
                    spawnDust.scale = 0.8f;
                }
                CheckForSpawning++;
            }

            // 检测右键状态
            CheckNestingState();

            if (isNesting)
            {
                NestingBehavior();
            }
            else
            {
                // 使用标准召唤物AI
                Projectile.CTFChargingMinionAI1(
                    1200f,   // maxDistanceFromTarget
                    1500f,   // chargeDistance
                    2200f,   // chargeDistanceMax
                    150f,    // distanceToAttack
                    0,       // projectileType (0 = 不发射弹幕)
                    20f,     // speed
                    8f,      // inertia
                    4f,      // attackCooldown
                    new Vector2(0f, -60f), // idlePosition
                    10f,     // chargeSpeedMultiplier
                    8f,      // chargeInertiaMultiplier
                    false,   // moveAsOne
                    false,   // ignoreTilesWhileCharging
                    0        // behavior (0 = 标准行为)
                );
            }

            // 动画
            UpdateAnimation();

            // 视觉效果
            VisualEffects();
        }

        private void CheckNestingState()
        {
            // 检测玩家是否按住右键
            if (Owner.controlUseItem && Owner.altFunctionUse == 2)
            {
                isNesting = true;
                nestTimer = 60; // 设置归巢计时器
            }
            else if (nestTimer <= 0)
            {
                isNesting = false;
            }
            else
            {
                nestTimer--;
            }
        }

        private void NestingBehavior()
        {
            // 归巢行为：飞到玩家附近并停止攻击
            Vector2 nestPosition = Owner.Center + new Vector2(0, -40);
            float distanceToNest = Vector2.Distance(Projectile.Center, nestPosition);

            if (distanceToNest > MaxNestDistance)
            {
                // 飞向归巢位置
                Vector2 direction = nestPosition - Projectile.Center;
                direction.Normalize();
                Projectile.velocity = (Projectile.velocity * 20f + direction * 6f) / 21f;
            }
            else
            {
                // 在归巢位置附近缓慢移动
                Projectile.velocity *= 0.95f;

                // 轻微浮动效果
                if (Main.rand.NextBool(30))
                {
                    Projectile.velocity += new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, -0.1f));
                }
            }

            // 应用归巢增益
            ApplyNestingBuff();
        }

        private void ApplyNestingBuff()
        {
            // 每只归巢的大山雀提供1点生命再生和5点防御
            CTFAccessoriesPlayer modPlayer = Owner.GetModPlayer<CTFAccessoriesPlayer>();
            modPlayer.nestingGreatTits++;

            // 直接应用增益效果
            Owner.lifeRegen += 1;
            Owner.statDefense += 5;
        }

        private void UpdateAnimation()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            // 根据移动方向调整面向
            if (Projectile.velocity.X > 0f)
            {
                Projectile.spriteDirection = 1;
            }
            else if (Projectile.velocity.X < 0f)
            {
                Projectile.spriteDirection = -1;
            }
        }

        private void VisualEffects()
        {
            // 归巢时的特殊效果
            if (isNesting && Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Grass,
                    0f, 0f, 100, default, 0.8f
                );
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }

            // 飞行时的粒子效果
            if (!isNesting && Main.rand.NextBool(10) && Projectile.velocity.Length() > 1f)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Cloud,
                    0f, 0f, 100, default, 0.5f
                );
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * -0.2f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new(0, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            Color drawColor = Projectile.GetAlpha(lightColor);
            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            return false;
        }
        // 动画更新方法
        public void DoAnimation()
        {
            Projectile.frameCounter++;

            // 根据是否归巢调整动画速度
            int frameSpeed = isNesting ? 10 : 6; // 归巢时动画较慢，攻击时动画较快

            // 更新动画帧
            Projectile.frame = Projectile.frameCounter / frameSpeed % Main.projFrames[Projectile.type];

            // 根据移动方向调整面向
            if (Projectile.velocity.X > 0f)
            {
                Projectile.spriteDirection = 1;
            }
            else if (Projectile.velocity.X < 0f)
            {
                Projectile.spriteDirection = -1;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity) => false;
    }
}