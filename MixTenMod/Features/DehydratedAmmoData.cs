using MixTenMod.i18n;

namespace MixTenMod.Features;

/// <summary>
/// 失水弹弹药数据
/// </summary>
public class DehydratedAmmoData
{
    /// <summary>
    /// 弹药ID
    /// </summary>
    public const string AmmoId = "MixTenMod.DehydratedAmmo";

    /// <summary>
    /// 弹药名称
    /// </summary>
    public static string AmmoName => I18n.Tr("item.dehydrated-ammo.name");

    /// <summary>
    /// 弹药描述
    /// </summary>
    public static string AmmoDescription => I18n.Tr("item.dehydrated-ammo.description");

    /// <summary>
    /// 弹药价格（一盒）
    /// </summary>
    public const int AmmoPrice = 700;

    /// <summary>
    /// 每盒弹药数量
    /// </summary>
    public const int AmmoPerBox = 12;

    /// <summary>
    /// 乌鸦防御半径（格数）
    /// </summary>
    public const int CrowDefenseRadius = 16;

    /// <summary>
    /// 浇水时间倍数
    /// </summary>
    public const float WateringTimeMultiplier = 2.0f;

    /// <summary>
    /// 伤害加成
    /// </summary>
    public const int DamageBonus = 5;

    /// <summary>
    /// 射程加成
    /// </summary>
    public const int RangeBonus = 32;
}
