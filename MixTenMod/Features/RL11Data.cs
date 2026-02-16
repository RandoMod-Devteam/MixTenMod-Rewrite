using MixTenMod.i18n;

namespace MixTenMod.Features;

/// <summary>
/// RL-11 枪械数据
/// </summary>
public class RL11Data
{
    /// <summary>
    /// 物品ID
    /// </summary>
    public const string ItemId = "MixTenMod.RL11";

    /// <summary>
    /// 物品名称
    /// </summary>
    public static string ItemName => I18n.Tr("item.rl11.name");

    /// <summary>
    /// 物品描述
    /// </summary>
    public static string ItemDescription => I18n.Tr("item.rl11.description");

    /// <summary>
    /// 物品价格
    /// </summary>
    public const int ItemPrice = 1200;

    /// <summary>
    /// 弹匣容量
    /// </summary>
    public const int MagazineCapacity = 12;

    /// <summary>
    /// 射击间隔（毫秒）
    /// </summary>
    public const int FireInterval = 150;

    /// <summary>
    /// 射程（像素）
    /// </summary>
    public const int Range = 256;

    /// <summary>
    /// 伤害值
    /// </summary>
    public const int Damage = 15;
}
