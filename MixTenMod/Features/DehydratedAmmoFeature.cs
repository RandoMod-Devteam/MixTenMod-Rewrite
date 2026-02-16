using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using MixTenMod.API;
using MixTenMod.i18n;

namespace MixTenMod.Features;

/// <summary>
/// 失水弹功能 - 实现IAmmo接口
/// </summary>
public class DehydratedAmmoFeature : IAmmo
{
    private readonly IModHelper Helper;
    private RL11Feature? _rl11Feature;

    // IAmmo接口实现
    public string AmmoId => DehydratedAmmoData.AmmoId;
    public string AmmoName => DehydratedAmmoData.AmmoName;
    public string AmmoDescription => DehydratedAmmoData.AmmoDescription;
    public int DamageBonus => DehydratedAmmoData.DamageBonus;
    public int RangeBonus => DehydratedAmmoData.RangeBonus;
    public AmmoSpecialEffect SpecialEffect => AmmoSpecialEffect.None;
    public float SpecialEffectValue => 0f;

    // 状态
    private bool _isActive;
    private int _activeTicks;
    private const int MaxActiveTicks = 3600; // 1分钟 (60 ticks/second * 60)

    public DehydratedAmmoFeature(IModHelper helper)
    {
        Helper = helper;
        _isActive = false;
        _activeTicks = 0;

        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        helper.Events.Display.MenuChanged += OnMenuChanged;
        helper.Events.Player.InventoryChanged += OnInventoryChanged;
    }

    /// <summary>
    /// 设置RL-11引用以注册弹药
    /// </summary>
    public void SetRL11(RL11Feature rl11)
    {
        _rl11Feature = rl11;
        _rl11Feature.RegisterAmmo(this);
    }

    /// <summary>
    /// 检查是否兼容指定枪械
    /// </summary>
    public bool IsCompatibleWith(string gunId)
    {
        return gunId == RL11Data.ItemId;
    }

    /// <summary>
    /// 激活失水弹效果
    /// </summary>
    public void Activate()
    {
        if (_isActive) return;

        _isActive = true;
        _activeTicks = 0;

        // 显示激活消息
        Game1.addHUDMessage(new HUDMessage(
            I18n.Tr("message.dehydrated-ammo.activate"),
            HUDMessage.newQuest_type
        ));

        // 播放激活音效
        Game1.playSound("debuffSpell");
    }

    /// <summary>
    /// 停用失水弹效果
    /// </summary>
    public void Deactivate()
    {
        if (!_isActive) return;

        _isActive = false;
        _activeTicks = 0;

        // 显示停用消息
        Game1.addHUDMessage(new HUDMessage(
            I18n.Tr("message.dehydrated-ammo.deactivate"),
            HUDMessage.newQuest_type
        ));
    }

    /// <summary>
    /// 更新逻辑
    /// </summary>
    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady) return;

        // 检查是否正在使用失水弹
        CheckAmmoUsage();

        // 更新活跃状态
        if (_isActive)
        {
            _activeTicks++;
            if (_activeTicks >= MaxActiveTicks)
            {
                Deactivate();
            }

            // 防御乌鸦
            DefendAgainstCrows();
        }
    }

    /// <summary>
    /// 检查弹药使用情况
    /// </summary>
    private void CheckAmmoUsage()
    {
        if (_rl11Feature == null) return;

        // 如果RL-11正在使用失水弹，激活效果
        if (_rl11Feature.CurrentAmmoType?.AmmoId == AmmoId && _rl11Feature.CurrentAmmo > 0)
        {
            if (!_isActive)
            {
                Activate();
            }
        }
        else if (_isActive)
        {
            // 如果切换了弹药类型或弹药耗尽，停用效果
            Deactivate();
        }
    }

    /// <summary>
    /// 防御乌鸦
    /// </summary>
    private void DefendAgainstCrows()
    {
        var player = Game1.player;
        if (player?.currentLocation == null) return;

        var location = player.currentLocation;
        Vector2 playerTile = player.Tile;

        // 检查范围内的作物
        for (int x = -DehydratedAmmoData.CrowDefenseRadius; x <= DehydratedAmmoData.CrowDefenseRadius; x++)
        {
            for (int y = -DehydratedAmmoData.CrowDefenseRadius; y <= DehydratedAmmoData.CrowDefenseRadius; y++)
            {
                Vector2 tile = new(playerTile.X + x, playerTile.Y + y);

                // 检查是否有作物
                if (location.terrainFeatures.TryGetValue(tile, out var feature) &&
                    feature is HoeDirt dirt &&
                    dirt.crop != null)
                {
                    // 防止乌鸦攻击
                    PreventCrowAttack(location, tile);
                }
            }
        }
    }

    /// <summary>
    /// 防止乌鸦攻击指定位置的作物
    /// </summary>
    private void PreventCrowAttack(GameLocation location, Vector2 tile)
    {
        // 在作物位置创建威慑效果
        if (Game1.random.NextDouble() < 0.01) // 1%概率每tick显示效果
        {
            location.temporarySprites.Add(new TemporaryAnimatedSprite(
                "TileSheets\\animations",
                new Rectangle(0, 128, 16, 16),
                100f,
                4,
                0,
                tile * 64f + new Vector2(32, 32),
                false,
                false
            )
            {
                scale = 0.5f,
                color = Color.LightBlue,
                alpha = 0.5f
            });
        }
    }

    /// <summary>
    /// 检查浇水时间是否应该翻倍
    /// </summary>
    public bool ShouldDoubleWateringTime()
    {
        return _isActive;
    }

    /// <summary>
        /// 商店菜单打开时添加弹药
        /// </summary>
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not ShopMenu shopMenu) return;

            // 在皮埃尔商店和Joja商店出售
            if (Game1.currentLocation?.Name == "SeedShop" || Game1.currentLocation?.Name == "JojaMart")
            {
                AddAmmoToShop(shopMenu);
            }
        }

    /// <summary>
    /// 向商店添加弹药
    /// </summary>
    private void AddAmmoToShop(ShopMenu shopMenu)
    {
        var ammo = new StardewValley.Object(DehydratedAmmoData.AmmoId, DehydratedAmmoData.AmmoPerBox)
        {
            Name = DehydratedAmmoData.AmmoName,
            Price = DehydratedAmmoData.AmmoPrice
        };

        var itemPriceAndStock = shopMenu.itemPriceAndStock;
        var forSale = shopMenu.forSale;

        if (itemPriceAndStock != null && forSale != null)
        {
            itemPriceAndStock[ammo] = new ItemStockInformation(
                price: DehydratedAmmoData.AmmoPrice,
                stock: int.MaxValue
            );
            forSale.Add(ammo);
        }
    }

    /// <summary>
    /// 库存变化时检查弹药
    /// </summary>
    private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        // 检查玩家是否获得了失水弹
        foreach (var item in e.Added)
        {
            if (item?.Name == DehydratedAmmoData.AmmoName)
            {
                // 自动装填到RL-11（如果持有）
                TryAutoLoad();
                break;
            }
        }
    }

    /// <summary>
    /// 尝试自动装填到RL-11
    /// </summary>
    private void TryAutoLoad()
    {
        if (_rl11Feature == null) return;

        var player = Game1.player;
        if (player?.CurrentItem?.Name != RL11Data.ItemName)
            return;

        // 如果RL-11没有弹药或当前弹药不是失水弹，尝试装填
        if (_rl11Feature.CurrentAmmo == 0 ||
            _rl11Feature.CurrentAmmoType?.AmmoId != AmmoId)
        {
            // 检查库存中是否有失水弹
            var ammoItem = player.Items.FirstOrDefault(i => i?.Name == DehydratedAmmoData.AmmoName);
            if (ammoItem != null)
            {
                int amount = Math.Min(ammoItem.Stack, RL11Data.MagazineCapacity);
                if (_rl11Feature.LoadAmmo(this, amount))
                {
                    // 消耗弹药
                    ammoItem.Stack -= amount;
                    if (ammoItem.Stack <= 0)
                    {
                        player.removeFirstOfThisItemFromInventory(ammoItem.QualifiedItemId);
                    }

                    Game1.addHUDMessage(new HUDMessage(
                        I18n.Tr("message.dehydrated-ammo.loaded", new Dictionary<string, string> { { "amount", amount.ToString() } }),
                        HUDMessage.newQuest_type
                    ));
                }
            }
        }
    }
}
