using System;
using System.Collections;
using System.Reflection;
using Calamitytwinklefragment.Content.Items.Weapons.Melee;
using Terraria.ModLoader;

namespace Calamitytwinklefragment.System
{
    /// <summary>
    /// 把“鸿蒙方舟 → 断斩之剪”注册进灾厄的咒术炼注列表。
    /// 灾厄没有公开的注册接口，但死灵炼注的转化表 EnchantmentManager.ItemUpgradeRelationship
    /// 是一个公开字典；在 PostSetupContent 里反射补一条即可让硫火女巫识别。
    /// </summary>
    public class ScissorBladeEnchantSystem : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                return;

            Type managerType = calamity.Code.GetType("CalamityMod.UI.CalamitasEnchants.EnchantmentManager");
            PropertyInfo relationshipProperty = managerType?.GetProperty(
                "ItemUpgradeRelationship",
                BindingFlags.Public | BindingFlags.Static);

            if (relationshipProperty?.GetValue(null) is not IDictionary relationship)
                return;

            int sourceType = ModContent.ItemType<CalamityMod.Items.Weapons.Melee.ArkoftheCosmos>();
            int resultType = ModContent.ItemType<ScissorBlade>();

            // 避免模组重载时重复写入。
            if (!relationship.Contains(sourceType))
                relationship[sourceType] = resultType;
        }
    }
}
