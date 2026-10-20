using Calamitytwinklefragment.Content.Items.Weapons.Mana.StardustLine;
using Calamitytwinklefragment.CTFplayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace Calamitytwinklefragment.System
{
    class StardustCooldownBarSystem : ModSystem
    {
        public class CooldownBarSystem : ModSystem
        {
            public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
            {
                // 找到绘制层索引
                int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
                if (resourceBarIndex != -1)
                {
                    // 在资源条上方添加冷却条
                    layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                        "YourMod: Cooldown Bar",
                        delegate
                        {
                            DrawCooldownBar();
                            return true;
                        },
                        InterfaceScaleType.UI
                    ));
                }
            }

            private static void DrawCooldownBar()
            {
                Player player = Main.LocalPlayer;
                CooldownTimePlayer modPlayer = player.GetModPlayer<CooldownTimePlayer>();

                if (modPlayer.cooldownTimer > 0)
                {
                    // 冷却条的位置和大小
                    Vector2 position = new(Main.screenWidth / 2, Main.screenHeight - 50);
                    int width = 100; // 冷却条宽度
                    int height = 20; // 冷却条高度
                    float cooldownRatio = (float)modPlayer.cooldownTimer / 600;

                    // 获取 MagicPixel 纹理
                    Texture2D magicPixel = TextureAssets.MagicPixel.Value;

                    // 绘制冷却条背景
                    Main.spriteBatch.Draw(
                        magicPixel,
                        new Rectangle((int)position.X, (int)position.Y, width, height),
                        Color.Gray // 背景颜色
                    );

                    // 绘制冷却条前景
                    Main.spriteBatch.Draw(
                        magicPixel,
                        new Rectangle((int)position.X, (int)position.Y, (int)(width * cooldownRatio), height),
                        Color.Lerp(Color.DarkGoldenrod, Color.Gold, cooldownRatio) // 前景颜色（黄色主题）
                    );
                }
            }
        }
    }
}
