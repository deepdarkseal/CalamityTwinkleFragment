using CalamityMod.Items;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Calamitytwinklefragment.Content.Projectiles.MinusLineProj;
using Terraria.Audio;
using CalamityMod.Rarities;
using CalamityMod;

namespace Calamitytwinklefragment.Content.Items.Weapons.Melee.MinusLine
{
    class TheStoryOfTheApocalypse : ModItem
    {
        public static readonly float RagePerSecond = 0.01f; // 1%
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 1208;
            Item.DamageType = DamageClass.Melee;
            Item.width = 180;
            Item.height = 184;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<DarkBlue>();
            Item.shoot = ModContent.ProjectileType <EtherOrb>(); // 默认发射的弹幕
            Item.autoReuse = true;
            Item.shootSpeed = 10f; // 弹幕发射速度
            Item.noUseGraphic = true; // 禁用默认武器贴图渲染
        }
        public override Vector2? HoldoutOffset() => new Vector2(48, 48);
        private float Minustip = 0;
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2f) // 右键攻击
            {
                Item.useTime = 18; // 右键攻击速度
                Item.useAnimation = 18; // 右键攻击动画速度
                Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
                Item.noUseGraphic = false; // 启用默认武器贴图渲染
                Item.useStyle = ItemUseStyleID.Swing; // 使用默认挥舞风格
                Item.noMelee = false;
                Item.autoReuse = true;
            }
            else // 左键攻击
            {
                Item.useTime = 24; // 左键攻击速度
                Item.useAnimation = 24; // 左键攻击动画速度
                Item.DamageType = DamageClass.Melee;
                Item.noUseGraphic = true; // 禁用默认武器贴图渲染
                Item.useStyle = ItemUseStyleID.Shoot; // 使用射击风格
                Item.noMelee = true;
                Item.autoReuse = true;
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2f)
            {
                // 当 Minustip 达到 3 时，生成 ApocalypseFire 弹幕
                if (Minustip >= 3)
                {
                    // 生成弹幕的参数
                    int projectileType = ModContent.ProjectileType<ApocalypseFire>();
                    int fireDamage = (int)(Item.damage * 1f); // 弹幕伤害（可调整）

                    // 弹幕生成逻辑
                    Vector2 playerCenter = player.Center;
                    Vector2 mouseWorld = Main.MouseWorld;
                    Vector2 direction = mouseWorld - playerCenter;
                    direction.Normalize();

                    // 弹幕生成参数
                    int numProjectiles = 21; // 弹幕数量
                    float spreadAngle = MathHelper.ToRadians(120); // 弹幕散布角度（120度）
                    float baseAngle = direction.ToRotation() - spreadAngle * 0.5f; // 起始角度

                    for (int i = 0; i < numProjectiles; i++)
                    {
                        // 计算弹幕的生成位置
                        float angle = baseAngle + spreadAngle * (i / (float)(numProjectiles - 1));
                        float radius = Main.rand.NextFloat(190f, 210f); // 随机半径
                        Vector2 spawnPosition = playerCenter + angle.ToRotationVector2() * radius;

                        // 计算弹幕方向（由玩家指向外）
                        Vector2 fireVelocity = angle.ToRotationVector2() * Main.rand.NextFloat(0f, 3f); // 速度随机性

                        // 生成弹幕
                        Projectile.NewProjectile(
                            source,
                            spawnPosition,
                            fireVelocity,
                            projectileType,
                            fireDamage,
                            knockback,
                            player.whoAmI
                        );
                    }
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item73, player.Center);
                    // 重置 Minustip
                    Minustip = 0;

                    return false; // 阻止默认弹幕发射
                }
                else
                {
                    Minustip += 1.5f;
                    if (Minustip > 3)
                    {
                        Minustip = 3;
                    }
                    return false; // 返回 false 防止默认弹幕发射
                }
            }
            else // 左键行为
            {
                int projectile = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ApocalypseRolling>(), damage, knockback, player.whoAmI);
                // 发射一个追踪镰刀
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<DeathsAscensionProjectile>(), damage/3, knockback, player.whoAmI);
                // 播放音效
                SoundEngine.PlaySound(SoundID.Item71, player.Center);
                // 发射 2 个以太球
                for (int i = 0; i < 2; i++)
                {
                    Vector2 etherVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(30)); // 随机分散角度
                    etherVelocity *= Main.rand.NextFloat(0.8f, 1.2f); // 随机速度
                    float etherX = position.X + Main.rand.NextFloat(-50f, 50f);
                    float etherY = position.Y + Main.rand.NextFloat(-50f, 50f);
                    Projectile.NewProjectile(source, new Vector2(etherX, etherY), etherVelocity, ModContent.ProjectileType<EtherOrb>(), (int)(damage * 0.4f), knockback / 2, player.whoAmI);
                }
                Minustip++;
                if (Minustip > 3)
                {
                    Minustip = 3;
                }
                return false; // 返回 false 防止默认弹幕发射
            }
        }
    }
}
