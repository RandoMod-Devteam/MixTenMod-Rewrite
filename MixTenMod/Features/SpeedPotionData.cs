using StardewValley;
using MixTenMod.i18n;

namespace MixTenMod.Features;

/// <summary>
/// 速度剂物品数据
/// </summary>
public class SpeedPotionData
{
    /// <summary>
    /// 物品ID
    /// </summary>
    public const string ItemId = "MixTenMod.SpeedPotion";

    /// <summary>
    /// 物品名称
    /// </summary>
    public static string ItemName => "MixTenMod.SpeedPotion";

    /// <summary>
    /// 获取显示名称
    /// </summary>
    public static string GetDisplayName() => I18n.Tr("item.speed-potion.name");

    /// <summary>
    /// 获取物品描述
    /// </summary>
    public static string GetDescription() => I18n.Tr("item.speed-potion.description");

    /// <summary>
    /// 物品价格
    /// </summary>
    public const int ItemPrice = 500;

    /// <summary>
    /// 最大叠加层数
    /// </summary>
    public const int MaxStack = 3;

    /// <summary>
    /// 每层增加的速度倍数
    /// </summary>
    public const float SpeedMultiplierPerStack = 1.0f;
}
