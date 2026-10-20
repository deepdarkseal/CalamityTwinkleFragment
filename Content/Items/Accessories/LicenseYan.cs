using System;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Calamitytwinklefragment.Content.Items.Accessories
{
    /// <summary>
    /// 不稳定的盐喵许可证：开发者饰品，召唤盐喵并维持其存在。
    /// 物品栏与掉落物使用程序化动画（翻页 + 呼吸 + 金色辉光）；
    /// 若以后把贴图换成横向多帧（每帧 14×14），会自动改为逐帧播放。
    /// </summary>
    class LicenseYan : ModItem
    {
        /// <summary>单帧尺寸；多帧贴图按此宽度横向切割。</summary>
        private const int FrameWidth = 14;
        private const int FrameHeight = 14;

        /// <summary>多帧贴图的播放速度（帧/秒）。</summary>
        private const float FramesPerSecond = 10f;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;

            // 掉落物像灵魂一样浮动、不受重力，和“不稳定的许可证”这一设定相配。
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = FrameWidth;
            Item.height = FrameHeight;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.value = Item.buyPrice(0, 8, 22, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 2 帧持续、每帧刷新：盐喵只要饰品在身就始终存在。
            player.AddBuff(BuffType<Buff.YanMiaoPower>(), 2);

            // 盐喵已经在场时不重复召唤；死亡或消失后才会补上。
            if (player.ownedProjectileCounts[ProjectileType<Projectiles.YanMiao>()] > 0)
                return;

            int damage = (int)(822 * player.GetDamage(DamageClass.Generic).ApplyTo(1f));

            Projectile.NewProjectile(
                player.GetSource_Accessory(Item),
                player.Center,
                Vector2.Zero,
                ProjectileType<Projectiles.YanMiao>(),
                damage,
                0f,
                player.whoAmI);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Type].Value;
            Rectangle source = GetCurrentFrame(texture);

            float time = Main.GlobalTimeWrappedHourly;

            // 呼吸缩放与轻微摇摆，让静态图标看起来是活的。
            float breathe = 1f + MathF.Sin(time * 2.2f) * 0.05f;
            float sway = MathF.Sin(time * 1.7f) * 0.10f;

            // 纸张翻面：X 轴压扁到 35% 再展开，模拟许可证被风翻动。
            float flip = MathF.Abs(MathF.Cos(time * 1.3f));
            float scaleX = scale * breathe * (0.35f + 0.65f * flip);
            float scaleY = scale * breathe;

            // 金色辉光在暖黄与橙色之间脉动。
            float pulse = (MathF.Sin(time * 2.6f) + 1f) * 0.5f;
            Color glowColor = Color.Lerp(new Color(255, 238, 120), new Color(255, 178, 60), pulse);

            Vector2 drawScale = new Vector2(scaleX, scaleY);

            // 外层辉光、主体、亮芯；三层叠出“发光许可证”的层次。
            spriteBatch.Draw(texture, position, source, glowColor * 0.35f, sway, origin, drawScale * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, source, drawColor, sway, origin, drawScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, source, Color.White * 0.16f, sway, origin, drawScale * 0.78f, SpriteEffects.None, 0f);

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Item item = Main.item[whoAmI];
            Texture2D texture = TextureAssets.Item[Type].Value;
            Rectangle source = GetCurrentFrame(texture);
            Vector2 position = item.Center - Main.screenPosition;
            Vector2 origin = new Vector2(source.Width, source.Height) / 2f;

            float time = Main.GlobalTimeWrappedHourly;

            // 每个掉落物用 whoAmI 做相位偏移，避免多个许可证同步翻页。
            float breathe = 1f + MathF.Sin(time * 2.2f + whoAmI) * 0.06f;
            float sway = rotation + MathF.Sin(time * 1.6f + whoAmI * 0.7f) * 0.18f;
            float flip = MathF.Abs(MathF.Cos(time * 1.3f + whoAmI * 0.7f));

            Vector2 drawScale = new Vector2(
                scale * breathe * (0.35f + 0.65f * flip),
                scale * breathe);

            float pulse = (MathF.Sin(time * 2.6f + whoAmI) + 1f) * 0.5f;
            Color glowColor = Color.Lerp(new Color(255, 238, 120), new Color(255, 178, 60), pulse);

            spriteBatch.Draw(texture, position, source, glowColor * 0.3f, sway, origin, drawScale * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, source, lightColor, sway, origin, drawScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, source, Color.White * 0.14f, sway, origin, drawScale * 0.78f, SpriteEffects.None, 0f);

            return false;
        }

        /// <summary>
        /// 当前应绘制的帧。单帧贴图直接返回整张图；
        /// 横向多帧贴图（宽 = 14 × 帧数）按时间轮播。
        /// </summary>
        private static Rectangle GetCurrentFrame(Texture2D texture)
        {
            int frameCount = Math.Max(1, texture.Width / FrameWidth);
            if (frameCount <= 1)
                return new Rectangle(0, 0, texture.Width, texture.Height);

            int frameIndex = (int)(Main.GlobalTimeWrappedHourly * FramesPerSecond) % frameCount;
            return new Rectangle(frameIndex * FrameWidth, 0, FrameWidth, FrameHeight);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.LicenseCat).
                AddIngredient(ItemType<PerennialBar>(), 99).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
