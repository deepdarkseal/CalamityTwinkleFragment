using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalamityMod.Particles;
using CalamityMod.Rarities;
using Calamitytwinklefragment.Content.Projectiles.ScissorBladeProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 断斩之剪：鸿蒙方舟的死灵炼注形态，原型为《斩服少女》的断斩之剪。
    /// 攻击循环参考 Calamity 的 ArkoftheCosmos：
    /// 左键五连剪击，前四段挥舞并甩出撕裂之针，第五段掷出剪刀（按住左键，松开时合拢）；
    /// 右键改为切腹：以自身生命换取无敌帧与充能；满充能时按住上键右键释放大撕裂。
    /// </summary>
    internal class ScissorBlade : ModItem
    {
        public const float MaxCharge = 15f;
        public const float ChargeDamageMultiplier = 1.35f;
        public const float NeedleDamageMultiplier = 0.7f;
        public const float SnapDamageMultiplier = 1.2f;
        public const float BlastDamageMultiplier = 0.5f;

        private const float SelfHarmRatio = 0.10f;
        private const int SelfHarmImmuneFrames = 60;
        private const int SelfHarmCooldownFrames = 60;

        // 与鸿蒙方舟一致：连击与充能挂在物品实例上，并通过 NetSend/NetReceive 同步。
        public float Combo;
        public float Charge;
        public int SelfHarmCooldown;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = Item.height = 80;
            Item.damage = 2200;                 // 高于鸿蒙方舟（1700），低于影魔毕业武器
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.noMelee = true;
            Item.noUseGraphic = true;           // 剪刀由弹幕绘制
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 9.5f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.value = Item.buyPrice(0, 48, 0, 0);
            Item.shoot = ProjectileID.PurificationPowder; // 占位，真正生成由 Shoot 接管
            Item.shootSpeed = 28f;
            Item.rare = ModContent.RarityType<CosmicPurple>();
        }

        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 15;

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            if (SelfHarmCooldown > 0)
                SelfHarmCooldown--;

            // 第五段进入投掷阶段：按住左键保持 channel，松开时由投掷弹幕判定合拢。
            Item.channel = Combo == 4f;
        }

        public override bool CanUseItem(Player player)
        {
            // 与鸿蒙方舟一致：上一段挥砍还没结束时不允许再次使用。
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<ScissorBladeSlash>());
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 aim = velocity.SafeNormalize(Vector2.UnitX * player.direction);

            if (player.altFunctionUse == 2)
            {
                // 满充能 + 按住上键：大撕裂。
                if (Charge > 0f && player.controlUp)
                {
                    Projectile.NewProjectile(
                        source,
                        player.Center + aim * 90f,
                        aim,
                        ModContent.ProjectileType<ScissorBladeRift>(),
                        (int)(damage * Charge * ChargeDamageMultiplier * BlastDamageMultiplier),
                        0f,
                        player.whoAmI,
                        Charge);

                    Charge = 0f;
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.3f, Volume = 0.9f }, player.Center);
                    return false;
                }

                // 切腹：替代原格挡，自伤换取无敌帧与满充能。
                if (SelfHarmCooldown <= 0)
                {
                    // 自伤为“当前生命”的 10%，且不会直接把自己切死。
                    int selfDamage = Math.Max(1, (int)(player.statLife * SelfHarmRatio));
                    selfDamage = Math.Min(selfDamage, Math.Max(1, player.statLife - 1));
                    player.statLife -= selfDamage;
                    player.immune = true;
                    player.immuneTime = Math.Max(player.immuneTime, SelfHarmImmuneFrames);

                    Charge = MaxCharge;
                    SelfHarmCooldown = SelfHarmCooldownFrames;

                    // 血液飞溅：一部分带重力落下，一部分向外喷散。
                    for (int i = 0; i < 40; i++)
                    {
                        Vector2 bloodVelocity = Main.rand.NextVector2Circular(6f, 6f);
                        Dust dust = Dust.NewDustPerfect(
                            player.Center + Main.rand.NextVector2Circular(18f, 26f),
                            DustID.Blood,
                            bloodVelocity,
                            100,
                            default,
                            Main.rand.NextFloat(1f, 1.8f));
                        dust.noGravity = Main.rand.NextBool(3);
                    }

                    // 红色能量飞溅与冲击环，让“切腹”的受击反馈更明显。
                    for (int i = 0; i < 8; i++)
                    {
                        GeneralParticleHandler.SpawnParticle(new SquishyLightParticle(
                            player.Center + Main.rand.NextVector2Circular(10f, 20f),
                            Main.rand.NextVector2Circular(6f, 6f),
                            Main.rand.NextFloat(0.3f, 0.6f),
                            Color.Red,
                            40,
                            1,
                            1.5f,
                            hueShift: 0.02f));

                        GeneralParticleHandler.SpawnParticle(new CritSpark(
                            player.Center + Main.rand.NextVector2Circular(10f, 20f),
                            Main.rand.NextVector2Circular(5f, 5f),
                            Color.White,
                            Color.DarkRed,
                            Main.rand.NextFloat(0.8f, 1.4f),
                            14,
                            0.05f,
                            3f));
                    }

                    GeneralParticleHandler.SpawnParticle(new PulseRing(player.Center, Vector2.Zero, Color.Red, 0.05f, 1.4f, 10));

                    // 受击音效 + 剪刀切入的音效。
                    SoundEngine.PlaySound(SoundID.PlayerHit, player.Center);
                    SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.6f }, player.Center);
                }

                return false;
            }

            if (Charge > 0f)
                damage = (int)(ChargeDamageMultiplier * damage);

            // 0/1 为交替上挥、下挥；2 为第五段的投掷。
            float scissorState = Combo == 4f ? 2f : Combo % 2f;

            if (scissorState == 2f)
            {
                // 第五段：交由剪击弹幕进入投掷 / 合拢状态机（与鸿蒙方舟一致）。
                Projectile.NewProjectile(
                    source,
                    player.Center,
                    aim,
                    ModContent.ProjectileType<ScissorBladeSlash>(),
                    damage,
                    knockback,
                    player.whoAmI,
                    2f,
                    Charge);
            }
            else
            {
                Projectile.NewProjectile(
                    source,
                    player.Center,
                    aim,
                    ModContent.ProjectileType<ScissorBladeSlash>(),
                    damage,
                    knockback,
                    player.whoAmI,
                    scissorState,
                    Charge);

                // 前四段各甩出一根撕裂之针，穿透 3 次。
                Projectile.NewProjectile(
                    source,
                    player.Center + aim * 20f,
                    aim * (Item.shootSpeed * 1.4f),
                    ModContent.ProjectileType<ScissorBladeNeedle>(),
                    (int)(damage * NeedleDamageMultiplier),
                    knockback,
                    player.whoAmI);
            }

            Combo += 1f;
            if (Combo > 4f)
                Combo = 0f;

            Charge = Math.Max(0f, Charge - 1f);
            return false;
        }

        public override ModItem Clone(Item newEntity)
        {
            ModItem clone = base.Clone(newEntity);
            if (clone is ScissorBlade cloned && newEntity.ModItem is ScissorBlade source)
            {
                cloned.Combo = source.Combo;
                cloned.Charge = source.Charge;
            }

            return clone;
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Combo);
            writer.Write(Charge);
        }

        public override void NetReceive(BinaryReader reader)
        {
            Combo = reader.ReadSingle();
            Charge = reader.ReadSingle();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(
                Mod,
                "ChargeInfo",
                Language.GetTextValue("Mods.Calamitytwinklefragment.Items.ScissorBlade.ChargeInfo", (int)Charge, (int)MaxCharge))
            {
                OverrideColor = new Color(255, 120, 150)
            });
        }
    }
}
