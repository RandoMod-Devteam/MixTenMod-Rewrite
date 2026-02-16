namespace MixTenMod.API;

/// <summary>
/// 弹药接口 - 供其他模组实现以添加自定义弹药
/// </summary>
public interface IAmmo
{
    /// <summary>
    /// 弹药ID
    /// </summary>
    string AmmoId { get; }

    /// <summary>
    /// 弹药名称
    /// </summary>
    string AmmoName { get; }

    /// <summary>
    /// 弹药描述
    /// </summary>
    string AmmoDescription { get; }

    /// <summary>
    /// 伤害加成
    /// </summary>
    int DamageBonus { get; }

    /// <summary>
    /// 射程加成（像素）
    /// </summary>
    int RangeBonus { get; }

    /// <summary>
    /// 特殊效果类型
    /// </summary>
    AmmoSpecialEffect SpecialEffect { get; }

    /// <summary>
    /// 特殊效果数值
    /// </summary>
    float SpecialEffectValue { get; }

    /// <summary>
    /// 是否兼容指定枪械
    /// </summary>
    /// <param name="gunId">枪械ID</param>
    /// <returns>是否兼容</returns>
    bool IsCompatibleWith(string gunId);
}

/// <summary>
/// 弹药特殊效果类型
/// </summary>
public enum AmmoSpecialEffect
{
    /// <summary>
    /// 无特殊效果
    /// </summary>
    None,

    /// <summary>
    /// 燃烧效果
    /// </summary>
    Fire,

    /// <summary>
    /// 冰冻效果
    /// </summary>
    Ice,

    /// <summary>
    /// 穿透效果
    /// </summary>
    Pierce,

    /// <summary>
    /// 爆炸效果
    /// </summary>
    Explosive,

    /// <summary>
    /// 毒素效果
    /// </summary>
    Poison,

    /// <summary>
    /// 电击效果
    /// </summary>
    Electric
}
