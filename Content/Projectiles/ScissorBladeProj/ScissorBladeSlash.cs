using System;
using System.IO;
using CalamityMod;
using CalamityMod.Particles;
using Calamitytwinklefragment.Content.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace Calamitytwinklefragment.Content.Projectiles.ScissorBladeProj
{
    /// <summary>
    /// 断斩之剪的剪击，逻辑参考 Calamity 的 ArkoftheCosmosSwungBlade。
    /// ai[0]（Combo）：0 上挥 / 1 下劈（甩出追踪雷）/ 2 投掷 / 3 合拢收回。
    /// ai[1]（Charge）：充能层数，影响剪刀大小与双刃绘制。
    /// </summary>
    internal class ScissorBladeSlash : ModProjectile
    {
        private bool initialized;
        private Vector2 direction;
        private Particle smear;

        public ref float Combo => ref Projectile.ai[0];
        public ref float Charge => ref Projectile.ai[1];
        public Player Owner => Main.player[Projectile.owner];

        public float MaxSwingTime => SwirlSwing ? 55f : 35f;
        public int SwingDirection => Combo == 0f ? 1 * Math.Sign(direction.X) : -1 * Math.Sign(direction.X);
        public bool SwirlSwing => Combo == 1f;
        private readonly float SwingWidth = MathHelper.PiOver2 * 1.5f;
        public Vector2 DistanceFromPlayer => direction * 30f;
        public float SwingCompletion => (MaxSwingTime - Projectile.timeLeft) / MaxSwingTime;

        public bool OwnerCanShoot => !Owner.CantUseHoldout() && Owner.HeldItem.type == ItemType<ScissorBlade>();
        public bool Thrown => Combo == 2f || Combo == 3f;
        public const float MaxThrowTime = 140f;
        public float ThrowCompletion => (MaxThrowTime - Projectile.timeLeft) / MaxThrowTime;
        public const float SnapWindowStart = 0.35f;
        public const float SnapWindowEnd = 0.75f;
        public float SnapEndTime => MaxThrowTime - MaxThrowTime * SnapWindowEnd;
        public float SnapEndCompletion => (SnapEndTime - Projectile.timeLeft) / SnapEndTime;
        public ref float ChanceMissed => ref Projectile.localAI[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.width = Projectile.height = 60;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 35;
            Projectile.timeLeft = 35;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float bladeLength = 172f * Projectile.scale;

            if (Thrown)
            {
                bool mainCollision = Collision.CheckAABBvAABBCollision(
                    targetHitbox.TopLeft(),
                    targetHitbox.Size(),
                    Projectile.Center - Vector2.One * bladeLength / 2f,
                    Vector2.One * bladeLength);

                if (Combo == 2f)
                    return mainCollision;

                Vector2 thrownBladeStart = Vector2.SmoothStep(Owner.Center, Projectile.Center, MathHelper.Clamp(SnapEndCompletion + 0.25f, 0f, 1f));
                bool thrownScissorCollision = Collision.CheckAABBvLineCollision(
                    targetHitbox.TopLeft(),
                    targetHitbox.Size(),
                    thrownBladeStart,
                    thrownBladeStart + direction * bladeLength);

                return mainCollision || thrownScissorCollision;
            }

            float collisionPoint = 0f;
            Vector2 holdPoint = DistanceFromPlayer.Length() * Projectile.rotation.ToRotationVector2();

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Owner.Center + holdPoint,
                Owner.Center + holdPoint + Projectile.rotation.ToRotationVector2() * bladeLength,
                24,
                ref collisionPoint);
        }

        // 挥舞动画曲线（与鸿蒙方舟一致）。
        public CurveSegment anticipation = new CurveSegment(EasingType.ExpOut, 0f, 0f, 0.15f);
        public CurveSegment thrust = new CurveSegment(EasingType.PolyInOut, 0.1f, 0.15f, 0.85f, 3);
        public CurveSegment hold = new CurveSegment(EasingType.Linear, 0.5f, 1f, 0.2f);
        internal float SwingRatio() => PiecewiseAnimation(SwingCompletion, new CurveSegment[] { anticipation, thrust, hold });

        public CurveSegment startup = new CurveSegment(EasingType.SineIn, 0f, 0f, 0.25f);
        public CurveSegment swing = new CurveSegment(EasingType.SineOut, 0.1f, 0.25f, 0.75f);
        internal float SwirlRatio() => PiecewiseAnimation(SwingCompletion, new CurveSegment[] { startup, swing });

        public CurveSegment shoot = new CurveSegment(EasingType.PolyIn, 0f, 1f, -0.2f, 3);
        public CurveSegment remain = new CurveSegment(EasingType.Linear, SnapWindowStart, 0.8f, 0f);
        public CurveSegment retract = new CurveSegment(EasingType.SineIn, SnapWindowEnd, 1f, -1f);
        internal float ThrowRatio() => PiecewiseAnimation(ThrowCompletion, new CurveSegment[] { shoot, remain, retract });

        public CurveSegment sizeCurve = new CurveSegment(EasingType.SineBump, 0f, 0f, 1f);
        internal float ThrowScaleRatio() => PiecewiseAnimation(ThrowCompletion, new CurveSegment[] { sizeCurve });

        public override void AI()
        {
            if (!initialized)
            {
                Projectile.timeLeft = (int)(Thrown ? MaxThrowTime : MaxSwingTime);
                direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.velocity = direction;
                Projectile.rotation = direction.ToRotation();

                if (SwirlSwing)
                    Projectile.localNPCHitCooldown = Math.Max(1, (int)(Projectile.localNPCHitCooldown / 4f));

                initialized = true;
                Projectile.netUpdate = true;
                Projectile.netSpam = 0;
            }

            if (!Thrown)
            {
                Projectile.Center = Owner.Center + DistanceFromPlayer;

                if (!SwirlSwing)
                {
                    // 普通上挥：短促的弧线。
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Lerp(
                        SwingWidth / 2f * SwingDirection,
                        -SwingWidth / 2f * SwingDirection,
                        SwingRatio());
                }
                else
                {
                    // 下劈：超过 360° 的大回旋，并在途中甩出 6 颗追踪雷。
                    float startRot = (MathHelper.Pi - MathHelper.PiOver4) * SwingDirection;
                    float endRot = -(MathHelper.TwoPi + MathHelper.PiOver4 * 1.5f) * SwingDirection;
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Lerp(startRot, endRot, SwirlRatio());

                    if (Owner.whoAmI == Main.myPlayer &&
                        (Projectile.timeLeft - 1) % Math.Max(1, (int)Math.Ceiling(MaxSwingTime / 6f)) == 0)
                    {
                        float adjustedBlastRotation = Projectile.rotation - MathHelper.PiOver4 * 1.15f * Owner.direction;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Owner.Center + adjustedBlastRotation.ToRotationVector2() * 10f,
                            adjustedBlastRotation.ToRotationVector2() * 20f,
                            ProjectileType<ScissorBladeBolt>(),
                            (int)(Projectile.damage * 0.7f / 6f),
                            0f,
                            Owner.whoAmI);
                    }
                }

                Projectile.scale = 1.2f + (float)Math.Sin(SwingRatio() * MathHelper.Pi) * 0.6f + (Charge / 10f) * 0.2f;
            }
            else
            {
                // 投掷：剪刀追着光标飞出去。
                Projectile.Center = Vector2.Lerp(Projectile.Center, Main.MouseWorld, 0.025f * ThrowRatio());
                Projectile.Center = Projectile.Center.MoveTowards(Main.MouseWorld, 20f * ThrowRatio());

                if ((Projectile.Center - Owner.Center).Length() > 760f)
                    Projectile.Center = Owner.Center + Owner.DirectionTo(Projectile.Center) * 760f;

                Projectile.rotation -= MathHelper.PiOver4 * 0.3f;
                Projectile.scale = 1f + ThrowScaleRatio() * 0.5f;

                if (Math.Abs(ThrowCompletion - SnapWindowEnd) <= 0.005f)
                    direction = Projectile.Center - Owner.Center;

                if (ThrowCompletion > SnapWindowEnd)
                    Projectile.Center = Owner.Center + direction * ThrowRatio();

                // 合拢窗口：松开左键，剪刀扣合。
                if (!OwnerCanShoot && Combo == 2f && ThrowCompletion >= SnapWindowStart - 0.1f &&
                    ThrowCompletion < SnapWindowEnd && ChanceMissed == 0f)
                {
                    if (Main.myPlayer == Owner.whoAmI)
                    {
                        float rotationOffset = MathHelper.TwoPi * Main.rand.NextFloat();

                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 boltDirection = (MathHelper.TwoPi * (i / 3f) + rotationOffset).ToRotationVector2();
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                Projectile.Center + boltDirection * 30f,
                                boltDirection * 20f,
                                ProjectileType<ScissorBladeBolt>(),
                                (int)(Projectile.damage * 0.1f),
                                0f,
                                Owner.whoAmI);
                        }

                        // 清空局部免疫，让已经命中过的敌人再吃一次合拢伤害。
                        Array.Clear(Projectile.localNPCImmunity, 0, Projectile.localNPCImmunity.Length);
                    }

                    Combo = 3f;
                    direction = Projectile.Center - Owner.Center;
                    Projectile.velocity = Projectile.rotation.ToRotationVector2();
                    Projectile.timeLeft = (int)SnapEndTime;
                    Projectile.localNPCHitCooldown = (int)SnapEndTime;
                    SoundEngine.PlaySound(SoundID.Item84, Projectile.Center);

                    // 合拢脉冲与红色斩线（参考鸿蒙方舟的脉冲/斩线粒子）。
                    GeneralParticleHandler.SpawnParticle(new PulseRing(Projectile.Center, Vector2.Zero, Color.Red, 0.05f, 1.8f, 8));
                    GeneralParticleHandler.SpawnParticle(new LineVFX(
                        Projectile.Center - direction.SafeNormalize(Vector2.UnitX) * 40f,
                        direction.SafeNormalize(Vector2.UnitX) * 80f,
                        0.2f,
                        Color.Red * 0.7f,
                        expansion: 250f)
                    {
                        Lifetime = 10
                    });
                }
                else if (!OwnerCanShoot && Combo == 2f && ChanceMissed == 0f)
                {
                    ChanceMissed = 1f;
                }

                if (Combo == 3f)
                {
                    // 收刀：沿合拢时的方向缓慢回到玩家手中。
                    float curveDownGently = MathHelper.Lerp(1f, 0.8f, 1f - (float)Math.Sqrt(1f - (float)Math.Pow(SnapEndCompletion, 2f)));
                    Projectile.Center = Owner.Center + direction * curveDownGently;
                    Projectile.scale = 1.5f;

                    float orientateProperly = (float)Math.Sqrt(1f - (float)Math.Pow(MathHelper.Clamp(SnapEndCompletion + 0.2f, 0f, 1f) - 1f, 2f));
                    float extraRotations = (direction.ToRotation() + MathHelper.PiOver4 > Projectile.velocity.ToRotation()) ? -MathHelper.TwoPi : 0f;
                    Projectile.rotation = MathHelper.Lerp(Projectile.velocity.ToRotation(), direction.ToRotation() + extraRotations, orientateProperly);
                }
            }

            // 让玩家看起来正握着剪刀。
            Owner.heldProj = Projectile.whoAmI;
            Owner.ChangeDir(Math.Sign(Projectile.velocity.X));
            Owner.itemRotation = Projectile.rotation;
            if (Owner.direction != 1)
                Owner.itemRotation -= MathHelper.Pi;
            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);

            if (Main.rand.NextBool(3))
            {
                Vector2 dustPosition = Thrown ? Projectile.Center : Owner.Center + Projectile.rotation.ToRotationVector2() * 80f;
                Dust dust = Dust.NewDustPerfect(
                    dustPosition,
                    DustID.Blood,
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    100,
                    default,
                    Main.rand.NextFloat(0.9f, 1.3f));
                dust.noGravity = true;
            }

            DoParticleEffects();
        }

        /// <summary>
        /// 参考鸿蒙方舟的粒子组合，统一换成红色系：
        /// 上挥用星火，下劈用红色烟幕 + 星火，投掷/合拢用暗红烟幕与火花。
        /// </summary>
        private void DoParticleEffects()
        {
            if (!Thrown)
            {
                if (!SwirlSwing)
                {
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 position = Owner.Center + Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(40f, 140f);
                        GeneralParticleHandler.SpawnParticle(new CritSpark(
                            position,
                            Main.rand.NextVector2Circular(2f, 2f),
                            Color.White,
                            Color.Red,
                            Main.rand.NextFloat(0.8f, 1.3f),
                            12,
                            0.05f,
                            3f));
                    }
                }
                else
                {
                    float opacity = MathHelper.Clamp((float)Math.Sin((SwirlRatio() - 0.2f) * MathHelper.Pi), 0f, 1f) * 0.3f;

                    if (smear == null)
                    {
                        smear = new CircularSmearSmokeyVFX(Owner.Center, Color.Red * opacity, Projectile.rotation, Projectile.scale * 2.4f);
                        GeneralParticleHandler.SpawnParticle(smear);
                    }
                    else
                    {
                        smear.Rotation = Projectile.rotation + MathHelper.PiOver4;
                        smear.Time = 0;
                        smear.Position = Owner.Center;
                        smear.Scale = Projectile.scale * 2.6f;
                        smear.Color = Color.Red * opacity;
                    }

                    if (Main.rand.NextBool())
                    {
                        float maxDistance = Projectile.scale * 78f;
                        Vector2 distance = Main.rand.NextVector2Circular(maxDistance, maxDistance);
                        Vector2 angularVelocity = distance.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 * Owner.direction) * 2f * (1f + distance.Length() / 15f);

                        GeneralParticleHandler.SpawnParticle(new CritSpark(
                            Owner.Center + distance,
                            Owner.velocity + angularVelocity,
                            Color.White,
                            Main.rand.NextBool(3) ? Color.DarkRed : Color.Red,
                            1f + distance.Length() / maxDistance,
                            10,
                            0.05f,
                            3f));
                    }
                }
            }
            else
            {
                Color smokeColor = Color.Lerp(Color.DarkRed, Color.Red, (float)Math.Sin(ThrowCompletion * MathHelper.Pi) * 0.5f + 0.5f);
                float opacity = (Combo == 3f
                    ? (float)Math.Sin(SnapEndCompletion * MathHelper.PiOver2 + MathHelper.PiOver2)
                    : (float)Math.Sin(ThrowCompletion * MathHelper.Pi)) * 0.5f;

                if (smear == null)
                {
                    smear = new CircularSmearSmokeyVFX(Projectile.Center, smokeColor * opacity, Projectile.rotation, Projectile.scale * 1.7f);
                    GeneralParticleHandler.SpawnParticle(smear);
                }
                else
                {
                    smear.Rotation = Projectile.rotation - 3.5f * MathHelper.PiOver4;
                    smear.Time = 0;
                    smear.Position = Projectile.Center;
                    smear.Scale = Projectile.scale * 1.65f;
                    smear.Color = smokeColor * opacity;
                }

                if (Main.rand.NextBool())
                {
                    Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(2f, 5f);
                    GeneralParticleHandler.SpawnParticle(new CritSpark(
                        Projectile.Center,
                        velocity,
                        Color.White,
                        Color.Red,
                        Main.rand.NextFloat(0.8f, 1.4f),
                        12,
                        0.05f,
                        3f));
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 合拢时的伤害加成（与鸿蒙方舟的 snapDamageMultiplier 对应）。
            if (Combo == 3f)
                modifiers.SourceDamage *= 1.2f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    target.Center,
                    DustID.Blood,
                    Main.rand.NextVector2Circular(4f, 4f),
                    100,
                    default,
                    Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = true;
            }

            // 命中时的红色能量泄漏（参考鸿蒙方舟的 SquishyLightParticle）。
            for (int i = 0; i < 5; i++)
            {
                Vector2 particleSpeed = (target.Center - Projectile.Center)
                    .SafeNormalize(Vector2.One)
                    .RotatedByRandom(MathHelper.PiOver4 * 0.8f)
                    * Main.rand.NextFloat(3.6f, 8f);

                GeneralParticleHandler.SpawnParticle(new SquishyLightParticle(
                    target.Center,
                    particleSpeed,
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Color.Red,
                    60,
                    2,
                    2.5f,
                    hueShift: 0.06f));
            }

            if (Combo == 3f)
                SoundEngine.PlaySound(SoundID.Item84 with { Volume = 0.8f }, Projectile.Center);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(initialized);
            writer.WriteVector2(direction);
            writer.Write(ChanceMissed);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            initialized = reader.ReadBoolean();
            direction = reader.ReadVector2();
            ChanceMissed = reader.ReadSingle();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Combo == 0f)
                DrawSwingScissors(lightColor, false);
            else if (Combo == 1f)
                DrawSwingScissors(lightColor, true);
            else
                DrawThrownScissors(lightColor);

            return false;
        }

        private void DrawSwingScissors(Color lightColor, bool swirl)
        {
            Texture2D frontBlade = Request<Texture2D>("CalamityMod/Projectiles/Melee/SunderingScissorsLeft").Value;
            Texture2D frontGlow = Request<Texture2D>("CalamityMod/Projectiles/Melee/SunderingScissorsLeftGlow").Value;
            Texture2D backBlade = Request<Texture2D>("CalamityMod/Projectiles/Melee/SunderingScissorsRight").Value;
            Texture2D backGlow = Request<Texture2D>("CalamityMod/Projectiles/Melee/SunderingScissorsRightGlow").Value;

            bool flipped = Owner.direction < 0;
            SpriteEffects flip = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float extraAngle = flipped ? MathHelper.PiOver2 : 0f;

            float drawRotation = Projectile.rotation + MathHelper.PiOver4 + extraAngle;
            Vector2 drawOffset = Owner.Center + Projectile.rotation.ToRotationVector2() * 10f - Main.screenPosition;
            Vector2 drawOrigin = new Vector2(flipped ? frontBlade.Width : 0f, frontBlade.Height);

            // 充能时画两片剪刀（双刃），否则只画单片。
            if (Charge > 0f)
            {
                Vector2 backOrigin = new Vector2(flipped ? 90f : 44f, 86f);
                Vector2 backPosition = Owner.Center + Projectile.rotation.ToRotationVector2() * 10f
                    + (Projectile.rotation.ToRotationVector2() * 56f
                    + (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 11f * Owner.direction) * Projectile.scale
                    - Main.screenPosition;

                Main.EntitySpriteDraw(backBlade, backPosition, null, lightColor, drawRotation, backOrigin, Projectile.scale, flip, 0);
                Main.EntitySpriteDraw(backGlow, backPosition, null, Color.Lerp(lightColor, Color.White, 0.75f), drawRotation, backOrigin, Projectile.scale, flip, 0);
            }

            Main.EntitySpriteDraw(frontBlade, drawOffset, null, lightColor, drawRotation, drawOrigin, Projectile.scale, flip, 0);
            Main.EntitySpriteDraw(frontGlow, drawOffset, null, Color.Lerp(lightColor, Color.White, 0.75f), drawRotation, drawOrigin, Projectile.scale, flip, 0);

            // 下劈时的红色弧光。
            if (swirl && SwingCompletion > 0.2f)
            {
                Texture2D smear = Request<Texture2D>("Calamitytwinklefragment/Content/Projectiles/ScissorBladeProj/ScissorBladeSlash").Value;
                float opacity = (float)Math.Sin(SwingCompletion * MathHelper.Pi) * 0.5f;
                float rotation = Projectile.velocity.ToRotation() + MathHelper.Pi
                    + (-MathHelper.PiOver4 * 0.5f + MathHelper.PiOver4 * 0.5f * SwingCompletion) * SwingDirection;

                Main.EntitySpriteDraw(
                    smear,
                    Owner.Center - Main.screenPosition,
                    null,
                    new Color(220, 35, 55) * opacity,
                    rotation,
                    smear.Size() / 2f,
                    Projectile.scale * 2.3f,
                    SpriteEffects.None,
                    0);
            }
        }

        private void DrawThrownScissors(Color lightColor)
        {
            Texture2D frontBlade = Request<Texture2D>("CalamityMod/Projectiles/Melee/SunderingScissorsLeft").Value;
            Texture2D frontGlow = Request<Texture2D>("CalamityMod/Projectiles/Melee/SunderingScissorsLeftGlow").Value;
            Texture2D backBlade = Request<Texture2D>("CalamityMod/Projectiles/Melee/SunderingScissorsRight").Value;
            Texture2D backGlow = Request<Texture2D>("CalamityMod/Projectiles/Melee/SunderingScissorsRightGlow").Value;

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 drawOrigin = new Vector2(32f, 86f);
            float drawRotation = Projectile.rotation + MathHelper.PiOver4;

            Vector2 backOrigin = new Vector2(44f, 86f);
            float backRotation = Projectile.rotation + MathHelper.Lerp(
                MathHelper.PiOver4,
                MathHelper.PiOver2 * 1.33f,
                MathHelper.Clamp(ThrowCompletion * 2f, 0f, 1f));

            if (Combo == 3f)
                backRotation = Projectile.rotation + MathHelper.Lerp(
                    MathHelper.PiOver2 * 1.33f,
                    MathHelper.PiOver4,
                    MathHelper.Clamp(SnapEndCompletion + 0.5f, 0f, 1f));

            Main.EntitySpriteDraw(backBlade, drawPosition, null, lightColor, backRotation, backOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(backGlow, drawPosition, null, Color.Lerp(lightColor, Color.White, 0.75f), backRotation, backOrigin, Projectile.scale, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(frontBlade, drawPosition, null, lightColor, drawRotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(frontGlow, drawPosition, null, Color.Lerp(lightColor, Color.White, 0.75f), drawRotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }
    }
}
