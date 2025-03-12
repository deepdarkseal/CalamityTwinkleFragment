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
            Item.shootSpeed = 30f; // ��Ļ�ٶ�
        }

        // ���ڼ�¼�������е��˵Ĵ���
        private int hitCounter = 0;
        // ���ڼ�¼�ϴλ��е��˵�ʱ��
        private int lastHitTime = 0;
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ���ӻ��д���
            hitCounter++;
            // ������д�������ĳ�����ޣ����������˺�����
            if (hitCounter > 5)
            {
                hitCounter = 4; // �����д���
            }
            // �����ϴλ���ʱ��
            lastHitTime = (int)Main.GameUpdateCount;

            // ���ݻ��д��������˺�
            Item.damage = 80 + hitCounter * 10; // ÿ�λ��������˺�
            // ���ɱ�ը��Ļ
            Projectile.NewProjectile(
                player.GetSource_OnHit(target), // �˺���Դ
                target.Center, // ��ըλ��
                Vector2.Zero, // �ٶȣ�0��ʾ��ֹ��
                ModContent.ProjectileType<DeepRedExplosion>(), // ��Ļ����
                (int)(Item.damage * 0.4f), // ��ը�˺���������壩
                0, // ����
                player.whoAmI // ��Ļ������
            );
        }

        public override void UpdateInventory(Player player)
        {
            // ����Ƿ񳬹�2��δ���е���
            if ((int)Main.GameUpdateCount - lastHitTime > 90) // 1.5��
            {
                hitCounter = 0; // ���û��д���
                Item.damage = 80; // ���ö����˺�
            }
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<HellfireFlamberge>();
            recipe.AddIngredient(ItemID.RedDye, 11); //Ⱦ��
            recipe.AddIngredient(ItemID.FragmentSolar, 26);
            recipe.AddTile(TileID.LunarCraftingStation);     // �ϳ�վ
            recipe.Register();
        }
    }
}
