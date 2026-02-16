using Microsoft.Xna.Framework;
using StardewValley;

namespace MixTenMod.API;

/// <summary>
/// 枪械接口 - 供其他模组与枪械系统交互
/// </summary>
public interface IGun
{
    /// <summary>
    /// 枪械ID
    /// </summary>
    string GunId { get; }

    /// <summary>
    /// 枪械名称
    /// </summary>
    string GunName { get; }

    /// <summary>
    /// 弹匣容量
    /// </summary>
    int MagazineCapacity { get; }

    /// <summary>
    /// 当前弹药数量
    /// </summary>
    int CurrentAmmo { get; }

    /// <summary>
    /// 是否正在装填
    /// </summary>
    bool IsReloading { get; }

    /// <summary>
    /// 当前使用的弹药类型
    /// </summary>
    IAmmo? CurrentAmmoType { get; }

    /// <summary>
    /// 注册兼容的弹药类型
    /// </summary>
    /// <param name="ammo">弹药接口</param>
    void RegisterAmmo(IAmmo ammo);

    /// <summary>
    /// 卸载弹药类型
    /// </summary>
    /// <param name="ammoId">弹药ID</param>
    void UnregisterAmmo(string ammoId);

    /// <summary>
    /// 获取所有兼容的弹药类型
    /// </summary>
    /// <returns>弹药列表</returns>
    IEnumerable<IAmmo> GetCompatibleAmmo();

    /// <summary>
    /// 装填指定类型的弹药
    /// </summary>
    /// <param name="ammo">弹药类型</param>
    /// <param name="amount">数量</param>
    /// <returns>是否成功</returns>
    bool LoadAmmo(IAmmo ammo, int amount);

    /// <summary>
    /// 射击
    /// </summary>
    /// <param name="shooter">射击者</param>
    /// <param name="targetPosition">目标位置</param>
    /// <returns>是否成功射击</returns>
    bool Fire(Farmer shooter, Vector2 targetPosition);

    /// <summary>
    /// 重新装填
    /// </summary>
    void Reload();
}

/// <summary>
/// 枪械API - 供其他模组获取枪械实例
/// </summary>
public interface IGunAPI
{
    /// <summary>
    /// 获取指定ID的枪械
    /// </summary>
    /// <param name="gunId">枪械ID</param>
    /// <returns>枪械实例，如果不存在则返回null</returns>
    IGun? GetGun(string gunId);

    /// <summary>
    /// 获取所有已注册的枪械
    /// </summary>
    /// <returns>枪械列表</returns>
    IEnumerable<IGun> GetAllGuns();

    /// <summary>
    /// 注册新的枪械
    /// </summary>
    /// <param name="gun">枪械实例</param>
    void RegisterGun(IGun gun);

    /// <summary>
    /// 枪械注册事件
    /// </summary>
    event EventHandler<GunRegisteredEventArgs>? GunRegistered;
}

/// <summary>
/// 枪械注册事件参数
/// </summary>
public class GunRegisteredEventArgs : EventArgs
{
    /// <summary>
    /// 注册的枪械
    /// </summary>
    public IGun Gun { get; }

    public GunRegisteredEventArgs(IGun gun)
    {
        Gun = gun;
    }
}
