using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Projectiles.Pets;
using Calamitytwinklefragment.Content.Items.Weapons.Mana.StardustLine;
using Calamitytwinklefragment.Content.NPCs.BossGod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Calamitytwinklefragment.Content.Items.CallOfBoss
{
    internal class PutridCauldron : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12; // 与其他BOSS召唤物相同的优先级
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(5, 0, 0, 0);
            Item.rare = ItemRarityID.Red;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false; // 不消耗，可以重复使用
            Item.noMelee = true;
        }

        public override bool CanUseItem(Player player)
        {
            // 只检查是否已经存在腐烂之主
            bool bossAlreadyExists = NPC.AnyNPCs(ModContent.NPCType<LordOfPutrescence>());

            if (bossAlreadyExists)
            {
                Main.NewText("腐烂之主已经存在！", Color.Red);
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                // 播放召唤音效
                SoundEngine.PlaySound(SoundID.Roar, player.position);

                // 在玩家上方生成BOSS
                _ = player.Center - new Vector2(0, 200f);
                int type = ModContent.NPCType<LordOfPutrescence>();

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // 单人或服务器端直接生成
                    NPC.SpawnOnPlayer(player.whoAmI, type);
                }
                else
                {
                    // 客户端通过数据包请求生成
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemType<Necroplasm>(), 77).
                AddIngredient(ItemType<UelibloomBar>(), 7).
                AddIngredient(ItemID.Cauldron, 1).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }

        public override void PostUpdate()
        {
            // 物品发出微弱的腐败光芒
            Lighting.AddLight(Item.Center, 0.2f, 0.5f, 0.1f);
        }
    }
}
