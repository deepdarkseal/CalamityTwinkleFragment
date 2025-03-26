using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.Pets;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Calamitytwinklefragment.Content.Projectiles.StardustLineProj;
using Calamitytwinklefragment.CTFplayer;
using Microsoft.Xna.Framework;
using System.Diagnostics.Contracts;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Calamitytwinklefragment.Content.Items.Weapons.Mana.StardustLine
{
    class FinalPray : ModItem
    {
        // 右键冷却时间
        private static readonly int CooldownTime = 600; // 10秒冷却（60 ticks = 1秒）
        private static float starf;
        public override void SetDefaults()
        {
            Item.width = 100;
            Item.height = 100;
            Item.damage = 156; // 基础伤害
            Item.DamageType = DamageClass.Magic; // 魔法武器
            Item.mana = 10; // 每次使用消耗的魔力值
            Item.useTime = 22; // 使用间隔
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot; // 射击类型
            Item.noMelee = true; // 不是近战武器
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(0, 30, 0, 0); // 物品价值
            Item.rare = ItemRarityID.Red; // 稀有度
            Item.autoReuse = true; // 自动连发
            Item.shoot = ProjectileType<PrayStar>(); // 默认发射的弹幕
            Item.shootSpeed = 30f; // 弹幕速度
            Item.staff[Item.type] = true;
            Item.scale = 0.5f;
        }
        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            
            if (DownedBossSystem.downedProvidence)
            {
                crit += 10f; // 增加暴击率
            }
            if (DownedBossSystem.downedPolterghast)
            {
                crit += 10f; // 增加暴击率
            }
        }
        public static float Starff()
        {
            if(NPC.downedMoonlord)
            {
                starf = 1.5f;
            }
            else
            {
                starf = 0.8f;
            }
            return starf;
        }
        // 左键发射星星弹幕
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2) // 左键
            {
                int starCount = 4; // 发射的星星数量
                starf = Starff();
                //Main.NewText($"1", Color.Yellow);
                for (int i = 0; i < starCount; i++)
                {
                    float spread = MathHelper.ToRadians(Main.rand.NextFloat(15f, 30f)); // 随机角度间隔;
                    // 随机速度（10~25）
                    float starSpeed = Main.rand.NextFloat(10f, 40f);
                    //Main.NewText($"{i}", Color.Blue);
                    // 计算弹幕方向
                    Vector2 starVelocity = velocity.RotatedBy(-spread + i * (spread * 2 / (starCount - 1))).SafeNormalize(Vector2.Zero) * starSpeed;

                    // 发射弹幕
                    Projectile.NewProjectile(
                        source,
                        position,
                        starVelocity,
                        ProjectileType<PrayStar>(),
                        (int)(Item.damage * starf),
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
                    // 查找玩家当前的黑洞弹幕
                    foreach (Projectile proj in Main.projectile)
                    {
                        if (proj.active && proj.owner == player.whoAmI && proj.type == ProjectileType<StarWishBlackHole>())
                        {
                            // 提前引爆逻辑
                            proj.timeLeft = 30;
                            SoundEngine.PlaySound(SoundID.Item8, player.Center);
                            return true;
                        }
                    }
                    return false; // 没有黑洞时禁用右键
                }

                // 检查魔力值（显示更精准的数值处理）
                int requiredMana = 300;
                if (player.statMana >= requiredMana)
                {
                    // 获取合法的弹幕来源
                    var source = player.GetSource_ItemUse(Item);

                    // 计算实际伤害（魔法伤害加成）
                    int holedamage = (int)(306 * 0.5f * player.GetDamage(DamageClass.Magic).ApplyTo(1f));

                    // 发射黑洞弹幕
                    if (Projectile.NewProjectile(
                        source,
                        player.Center,
                        Vector2.Zero,
                        ProjectileType<StarWishBlackHole>(),
                        holedamage,
                        0,
                        player.whoAmI
                    ) == Main.maxProjectiles)
                    {
                        // 弹幕发射失败时的保护逻辑
                        return false;
                    }

                    // 更新状态
                    modPlayer.cooldownTimer = CooldownTime;
                    player.statMana -= requiredMana;
                    SoundEngine.PlaySound(SoundID.Item15, player.Center);
                    return true;
                }
                return false; // 魔力不足
            }
            return true; // 左键正常使用
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
                AddIngredient<StardustFaling>().
                AddIngredient<AlulaAustralis>().
                AddIngredient(ItemType<MeldConstruct>(), 8).
                AddIngredient(ItemID.FragmentStardust, 12).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
