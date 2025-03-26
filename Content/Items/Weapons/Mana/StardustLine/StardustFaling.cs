using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Rarities;
using Calamitytwinklefragment.Content.Projectiles.StardustLineProj;
using Calamitytwinklefragment.CTFplayer;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Calamitytwinklefragment.Content.Items.Weapons.Mana.StardustLine
{
    class StardustFaling : ModItem
    {
        // 右键冷却时间
        private static readonly int CooldownTime = 600; // 10秒冷却（60 ticks = 1秒）
        private static float starf;
        public override void SetDefaults()
        {
            Item.width = 100;
            Item.height = 100;
            Item.damage = 100; // 基础伤害
            Item.DamageType = DamageClass.Magic; // 魔法武器
            Item.mana = 10; // 每次使用消耗的魔力值
            Item.useTime = 22; // 使用间隔
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot; // 射击类型
            Item.noMelee = true; // 不是近战武器
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(0, 1, 0, 0); // 物品价值
            Item.rare = RarityType<Violet>();
            Item.autoReuse = true; // 自动连发
            Item.shoot = ProjectileType<PrayStar>(); // 默认发射的弹幕
            Item.shootSpeed = 30f; // 弹幕速度
            Item.staff[Item.type] = true;
            Item.scale = 0.5f;
        }
        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            if (DownedBossSystem.downedBrimstoneElemental)
            {
                crit += 8f; // 增加暴击率
            }
            if (DownedBossSystem.downedCryogen)
            {
                crit += 8f; // 增加暴击率
            }
            if (DownedBossSystem.downedAquaticScourge)
            {
                crit += 8f; // 增加暴击率
            }
        }
        public static float Starff()
        {
            if (DownedBossSystem.downedCalamitasClone)
            {
                starf = 0.9f;
            }
            else
            {
                starf = 0.5f;
            }
            return starf;
        }
        // 左键发射星星弹幕
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2) // 左键
            {
                starf = Starff();
                int starCount = 2; // 发射的星星数量

                for (int i = 0; i < starCount; i++)
                {
                    // 随机速度（10~25）
                    float starSpeed = Main.rand.NextFloat(10f, 30f);
                    float spread = MathHelper.ToRadians(Main.rand.NextFloat(5f, 15f)); // 随机角度间隔;
                    // 计算弹幕方向
                    Vector2 starVelocity = velocity.RotatedBy(-spread + i * (spread * 2 / (starCount - 1))).SafeNormalize(Vector2.Zero) * starSpeed;

                    // 发射弹幕
                    Projectile.NewProjectile(
                        source,
                        position,
                        starVelocity,
                        ProjectileType<PrayStar>(),
                        (int)(damage * starf),
                        knockback,
                        player.whoAmI
                    );
                }

                // 播放音效
                SoundEngine.PlaySound(SoundID.Item9, player.Center);

                return false;
            }
            return false;
        }

        // 右键发射黑洞弹幕
        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            // 获取当前玩家的 ModPlayer 实例
            CooldownTimePlayer modPlayer = player.GetModPlayer<CooldownTimePlayer>();
            if (player.altFunctionUse == 2) // 右键
            {
                // 检查冷却状态
                if (modPlayer.cooldownTimer > 0)
                {
                    return false; // 冷却时禁用右键
                }

                // 检查魔力值
                int requiredMana = 100;
                if (player.statMana >= requiredMana)
                {
                    var source = player.GetSource_ItemUse(Item);
                    int Astralstardamage = (int)(100 * 3f * player.GetDamage(DamageClass.Magic).ApplyTo(1f));

                    // 获取鼠标的世界坐标
                    Vector2 mousePos = Main.MouseWorld;

                    // 生成12颗星星
                    for (int i = 0; i < 12; i++)
                    {
                        // 从屏幕上方随机位置生成（Y坐标为屏幕顶部-200像素）
                        Vector2 spawnPos = new(
                            Main.rand.Next((int)(player.Center.X - Main.screenWidth / 2), (int)(player.Center.X + Main.screenWidth / 2)),
                            player.Center.Y - Main.screenHeight / 2 - 200
                        );

                        // 计算朝向鼠标的向量并添加随机散布
                        Vector2 direction = mousePos - spawnPos;
                        direction = direction.RotatedByRandom(MathHelper.ToRadians(5)); // 15度随机散布
                        direction.Normalize();

                        // 设置速度（基础速度+随机速度）
                        Vector2 velocity = direction * 30f + Main.rand.NextVector2Circular(3f, 3f);

                        Projectile.NewProjectile(
                            source,
                            spawnPos,
                            velocity,
                            ProjectileType<AstralStar>(),
                            Astralstardamage,
                            5f, // 根据需求设置击退值
                            player.whoAmI
                        );
                    }

                    modPlayer.cooldownTimer = CooldownTime;
                    player.statMana -= requiredMana;
                    SoundEngine.PlaySound(SoundID.Item9, player.Center); // 使用更合适的星尘音效
                    return true;
                }
                return false;
            }
            return true;
        }
        public override void UpdateInventory(Player player)
        {
            // 获取当前玩家的 ModPlayer 实例
            CooldownTimePlayer modPlayer = player.GetModPlayer<CooldownTimePlayer>();
            if (modPlayer.cooldownTimer == 1)
                SoundEngine.PlaySound(SoundID.Item35, player.Center);
        }

        public override void AddRecipes()
        {
            CreateRecipe().	
                AddIngredient(ItemID.FallenStar, 8).                
                AddIngredient(ItemType<StarblightSoot>(), 12).
                AddIngredient(ItemID.MeteoriteBar, 10).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
