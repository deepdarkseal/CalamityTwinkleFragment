using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using System.Reflection;
using static CalamityMod.CalPlayer.CalamityPlayer;

namespace Calamitytwinklefragment.Content.TFTools
{
    public static class AdrenalineHelper
    {
        // 获取 CalamityPlayer 类型
        private static readonly Type calamityPlayerType = null;

        // 获取 adrenaline 字段
        private static readonly FieldInfo adrenalineField = null;

        // 获取 adrenalineMax 字段
        private static readonly FieldInfo adrenalineMaxField = null;

        static AdrenalineHelper()
        {
            // 确保 CalamityMod 已加载
            Mod calamityMod = ModLoader.GetMod("CalamityMod");
            if (calamityMod != null)
            {
                // 通过 Assembly 获取 CalamityPlayer 类型
                calamityPlayerType = calamityMod.Code?.GetType("CalamityMod.CalPlayer.CalamityPlayer");
                if (calamityPlayerType != null)
                {
                    // 获取字段
                    adrenalineField = calamityPlayerType.GetField("adrenaline", BindingFlags.NonPublic | BindingFlags.Instance);
                    adrenalineMaxField = calamityPlayerType.GetField("adrenalineMax", BindingFlags.NonPublic | BindingFlags.Instance);
                }
            }
        }

        // 获取 adrenaline 值
        public static float GetAdrenaline(Player player)
        {
            if (calamityPlayerType == null || adrenalineField == null)
            {
                ModLoader.GetMod("Calamitytwinklefragment").Logger.Warn("CalamityMod or adrenaline field not found!");
                return 0f;
            }

            // 通过反射调用 player.GetModPlayer(Type)
            MethodInfo getModPlayerMethod = typeof(Player).GetMethod("GetModPlayer", [typeof(Type)]);
            if (getModPlayerMethod == null)
            {
                ModLoader.GetMod("Calamitytwinklefragment").Logger.Warn("Player.GetModPlayer(Type) method not found!");
                return 0f;
            }

            // 调用 player.GetModPlayer(calamityPlayerType)
            object calamityPlayer = getModPlayerMethod.Invoke(player, [calamityPlayerType]);

            if (calamityPlayer == null)
            {
                ModLoader.GetMod("Calamitytwinklefragment").Logger.Warn("CalamityPlayer instance not found!");
                return 0f;
            }

            // 获取 adrenaline 值
            return (float)adrenalineField.GetValue(calamityPlayer);
        }

        // 获取 adrenalineMax 值
        public static float GetAdrenalineMax(Player player)
        {
            if (calamityPlayerType == null || adrenalineMaxField == null)
            {
                ModLoader.GetMod("Calamitytwinklefragment").Logger.Warn("CalamityMod or adrenalineMax field not found!");
                return 0f;
            }

            // 通过反射调用 player.GetModPlayer(Type)
            MethodInfo getModPlayerMethod = typeof(Player).GetMethod("GetModPlayer", [typeof(Type)]);
            if (getModPlayerMethod == null)
            {
                ModLoader.GetMod("Calamitytwinklefragment").Logger.Warn("Player.GetModPlayer(Type) method not found!");
                return 0f;
            }

            // 调用 player.GetModPlayer(calamityPlayerType)
            object calamityPlayer = getModPlayerMethod.Invoke(player, [calamityPlayerType]);

            if (calamityPlayer == null)
            {
                ModLoader.GetMod("Calamitytwinklefragment").Logger.Warn("CalamityPlayer instance not found!");
                return 0f;
            }

            // 获取 adrenalineMax 值
            return (float)adrenalineMaxField.GetValue(calamityPlayer);
        }
    }
}