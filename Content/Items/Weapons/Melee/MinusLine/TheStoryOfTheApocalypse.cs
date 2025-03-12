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

namespace Calamitytwinklefragment.Content.Items.Weapons.Melee.MinusLine
{
    class TheStoryOfTheApocalypse : ModItem
    {
        public override void SetStaticDefaults()
        {
            _ = ItemID.Sets.ItemsThatAllowRepeatedRightClick;
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
        private float Minustip = 0;
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2f) // 右键攻击
            {
                Item.useTime = 24; // 右键攻击速度
                Item.useAnimation = 24; // 右键攻击动画速度
                Item.noUseGraphic = false; // 启用默认武器贴图渲染
                Item.useStyle = ItemUseStyleID.Swing; // 使用默认挥舞风格
                Item.noMelee = false;
                Item.autoReuse = true;
            }
            else // 左键攻击
            {
                Item.useTime = 24; // 左键攻击速度
                Item.useAnimation = 24; // 左键攻击动画速度
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
                Minustip += 1.5f;
                if (Minustip > 3)
                {
                    Minustip = 3;
                }
                return false; // 返回 false 防止默认弹幕发射
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
                if (Minustip > 2)
                {
                    Minustip = 2;
                }
                return false; // 返回 false 防止默认弹幕发射
            }
        }
    }
}
