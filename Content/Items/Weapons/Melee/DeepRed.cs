using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Melee;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Calamitytwinklefragment.Content.Projectiles;

namespace Calamitytwinklefragment.Content.Items.Weapons.Melee
{
    public class DeepRed : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 80;
            Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Item.width = 170;
            Item.height = 200;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = CalamityGlobalItem.RarityCyanBuyPrice;
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shootSpeed = 30f; // 弹幕速度
        }

        // 用于记录连续击中敌人的次数
        private int hitCounter = 0;
        // 用于记录上次击中敌人的时间
        private int lastHitTime = 0;
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 增加击中次数
            hitCounter++;
            // 如果击中次数超过某个上限，可以限制伤害增长
            if (hitCounter > 5)
            {
                hitCounter = 4; // 最大击中次数
            }
            // 更新上次击中时间
            lastHitTime = (int)Main.GameUpdateCount;

            // 根据击中次数增加伤害
            Item.damage = 80 + hitCounter * 10; // 每次击中增加伤害
            // 生成爆炸弹幕
            Projectile.NewProjectile(
                player.GetSource_OnHit(target), // 伤害来源
                target.Center, // 爆炸位置
                Vector2.Zero, // 速度（0表示静止）
                ModContent.ProjectileType<DeepRedExplosion>(), // 弹幕类型
                (int)(Item.damage * 0.4f), // 爆炸伤害（武器面板）
                0, // 击退
                player.whoAmI // 弹幕所有者
            );
        }

        public override void UpdateInventory(Player player)
        {
            // 检查是否超过2秒未击中敌人
            if ((int)Main.GameUpdateCount - lastHitTime > 90) // 1.5秒
            {
                hitCounter = 0; // 重置击中次数
                Item.damage = 80; // 重置额外伤害
            }
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<HellfireFlamberge>();
            recipe.AddIngredient(ItemID.RedDye, 11); //染料
            recipe.AddIngredient(ItemID.FragmentSolar, 26);
            recipe.AddTile(TileID.LunarCraftingStation);     // 合成站
            recipe.Register();
        }
    }
}
