using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MixTenMod.API;
using MixTenMod.Content;
using MixTenMod.i18n;
using MixTenMod.UI;

namespace MixTenMod.Features;

/// <summary>
/// RL-11 枪械功能
/// </summary>
public class RL11Feature : IGun
{
    private readonly IModHelper Helper;
    private readonly ModContent? _modContent;
    private readonly List<IAmmo> _compatibleAmmo = new();
    private readonly GunGUI _gui;

    public string GunId => RL11Data.ItemId;
    public string GunName => RL11Data.ItemName;
    public int MagazineCapacity => RL11Data.MagazineCapacity;
    public int CurrentAmmo { get; private set; }
    public bool IsReloading { get; private set; }
    public IAmmo? CurrentAmmoType { get; private set; }
    
    // 能量系统
    public int CurrentEnergy { get; private set; }
    public int MaxEnergy { get; private set; } = 100;
    public const int EnergyPerShot = 3;

    private int _lastFireTime;
    private bool _isHoldingGun;
    private int _reloadStartTime;
    private Vector2 _mousePosition;
    private bool _showTargetHighlight;

    public RL11Feature(IModHelper helper, ModContent? modContent = null)
    {
        Helper = helper;
        _modContent = modContent;
        CurrentAmmo = 0;
        CurrentEnergy = MaxEnergy;
        IsReloading = false;
        _lastFireTime = 0;
        _isHoldingGun = false;
        _showTargetHighlight = false;
        _gui = new GunGUI(helper, this);

        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        helper.Events.Display.MenuChanged += OnMenuChanged;
        helper.Events.Input.ButtonPressed += OnButtonPressed;
        helper.Events.Input.CursorMoved += OnCursorMoved;
        helper.Events.Display.RenderedHud += OnRenderedHud;
        helper.Events.Display.RenderedWorld += OnRenderedWorld;
    }

    /// <summary>
    /// 注册兼容的弹药类型
    /// </summary>
    public void RegisterAmmo(IAmmo ammo)
    {
        if (!_compatibleAmmo.Any(a => a.AmmoId == ammo.AmmoId))
        {
            _compatibleAmmo.Add(ammo);
        }
    }

    /// <summary>
    /// 卸载弹药类型
    /// </summary>
    public void UnregisterAmmo(string ammoId)
    {
        var ammo = _compatibleAmmo.FirstOrDefault(a => a.AmmoId == ammoId);
        if (ammo != null)
        {
            _compatibleAmmo.Remove(ammo);
        }
    }

    /// <summary>
    /// 获取所有兼容的弹药类型
    /// </summary>
    public IEnumerable<IAmmo> GetCompatibleAmmo()
    {
        return _compatibleAmmo.AsReadOnly();
    }

    /// <summary>
    /// 装填指定类型的弹药
    /// </summary>
    public bool LoadAmmo(IAmmo ammo, int amount)
    {
        if (!ammo.IsCompatibleWith(GunId))
            return false;

        CurrentAmmoType = ammo;
        CurrentAmmo = Math.Min(amount, MagazineCapacity);
        return true;
    }

    /// <summary>
    /// 射击
    /// </summary>
    public bool Fire(Farmer shooter, Vector2 targetPosition)
    {
        // 检查能量是否足够
        if (CurrentEnergy < EnergyPerShot)
        {
            Game1.showRedMessage(I18n.Tr("message.rl11.no-energy"));
            return false;
        }

        if (CurrentAmmo <= 0 || IsReloading)
            return false;

        int currentTime = Environment.TickCount;
        if (currentTime - _lastFireTime < RL11Data.FireInterval)
            return false;

        _lastFireTime = currentTime;
        
        // 消耗子弹和能量
        CurrentAmmo--;
        CurrentEnergy -= EnergyPerShot;

        // 播放射击音效
        _modContent?.PlaySound("RifleShot");

        // 计算伤害
        int damage = RL11Data.Damage;
        if (CurrentAmmoType != null)
        {
            damage += CurrentAmmoType.DamageBonus;
        }

        // 发射子弹
        FireProjectile(shooter, targetPosition, damage);

        // 应用后坐力效果
        ApplyRecoil(shooter);

        return true;
    }

    /// <summary>
    /// 重新装填
    /// </summary>
    public void Reload()
    {
        if (IsReloading || CurrentAmmo >= MagazineCapacity)
            return;

        IsReloading = true;
        _reloadStartTime = Environment.TickCount;
        _modContent?.PlaySound("ReloadStart");
    }

    /// <summary>
    /// 完成装填
    /// </summary>
    private void FinishReload()
    {
        CurrentAmmo = MagazineCapacity;
        IsReloading = false;
        _modContent?.PlaySound("ReloadEnd");
    }

    /// <summary>
    /// 发射子弹
    /// </summary>
    private void FireProjectile(Farmer shooter, Vector2 targetPosition, int damage)
    {
        var location = shooter.currentLocation;
        if (location == null) return;

        Vector2 startPosition = shooter.Position + new Vector2(32, 32);
        Vector2 direction = targetPosition - startPosition;
        direction.Normalize();

        // 创建子弹精灵
        float rotation = (float)Math.Atan2(direction.Y, direction.X);
        int range = RL11Data.Range;
        if (CurrentAmmoType != null)
        {
            range += CurrentAmmoType.RangeBonus;
        }

        // 添加临时子弹精灵
        location.temporarySprites.Add(new TemporaryAnimatedSprite(
            "TileSheets\\animations",
            new Rectangle(0, 0, 16, 16),
            50f,
            1,
            0,
            startPosition,
            false,
            false
        )
        {
            rotation = rotation,
            motion = direction * 15f,
            scale = 1.5f,
            color = CurrentAmmoType?.SpecialEffect switch
            {
                AmmoSpecialEffect.Fire => Color.OrangeRed,
                AmmoSpecialEffect.Ice => Color.Cyan,
                AmmoSpecialEffect.Electric => Color.Yellow,
                AmmoSpecialEffect.Poison => Color.Green,
                _ => Color.White
            }
        });

        // 检测命中
        CheckHit(location, startPosition, direction, range, damage);
    }

    /// <summary>
    /// 检测命中
    /// </summary>
    private void CheckHit(GameLocation location, Vector2 start, Vector2 direction, int range, int damage)
    {
        for (int i = 0; i < range; i += 8)
        {
            Vector2 checkPos = start + direction * i;
            Vector2 tilePos = new((int)(checkPos.X / 64), (int)(checkPos.Y / 64));

            // 检查怪物
            foreach (var character in location.characters)
            {
                if (character is StardewValley.Monsters.Monster monster &&
                    Vector2.Distance(monster.Position, checkPos) < 32)
                {
                    // 造成伤害
                    monster.takeDamage(damage, 0, 0, false, 0.0, Game1.player);

                    // 应用特殊效果
                    ApplySpecialEffect(monster);
                    return;
                }
            }

            // 检查障碍物
            if (location.isTerrainFeatureAt((int)tilePos.X, (int)tilePos.Y))
                break;
        }
    }

    /// <summary>
    /// 应用特殊效果
    /// </summary>
    private void ApplySpecialEffect(StardewValley.Monsters.Monster monster)
    {
        if (CurrentAmmoType == null) return;

        switch (CurrentAmmoType.SpecialEffect)
        {
            case AmmoSpecialEffect.Fire:
                // 燃烧效果 - 持续伤害
                ApplyFireEffect(monster);
                break;

            case AmmoSpecialEffect.Ice:
                // 冰冻效果 - 减速
                ApplyIceEffect(monster);
                break;

            case AmmoSpecialEffect.Explosive:
                // 爆炸效果 - 范围伤害
                ApplyExplosiveEffect(monster);
                break;
        }
    }

    /// <summary>
    /// 应用燃烧效果
    /// </summary>
    private void ApplyFireEffect(StardewValley.Monsters.Monster monster)
    {
        int fireDamage = (int)CurrentAmmoType!.SpecialEffectValue;
        int ticks = 0;

        Helper.Events.GameLoop.UpdateTicked += OnFireTick;

        void OnFireTick(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || monster.currentLocation == null)
            {
                Helper.Events.GameLoop.UpdateTicked -= OnFireTick;
                return;
            }

            if (e.Ticks % 60 == 0) // 每秒触发一次
            {
                ticks++;
                if (ticks <= 3)
                {
                    monster.takeDamage(fireDamage, 0, 0, false, 0.0, Game1.player);
                }
                else
                {
                    Helper.Events.GameLoop.UpdateTicked -= OnFireTick;
                }
            }
        }
    }

    /// <summary>
    /// 应用冰冻效果
    /// </summary>
    private void ApplyIceEffect(StardewValley.Monsters.Monster monster)
    {
        float originalSpeed = monster.Speed;
        monster.Speed = (int)(monster.Speed * CurrentAmmoType!.SpecialEffectValue);

        int elapsed = 0;
        Helper.Events.GameLoop.UpdateTicked += OnIceTick;

        void OnIceTick(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || monster.currentLocation == null)
            {
                Helper.Events.GameLoop.UpdateTicked -= OnIceTick;
                return;
            }

            elapsed++;
            if (elapsed >= 180) // 3秒后恢复 (60 ticks/second * 3)
            {
                monster.Speed = (int)originalSpeed;
                Helper.Events.GameLoop.UpdateTicked -= OnIceTick;
            }
        }
    }

    /// <summary>
    /// 应用爆炸效果
    /// </summary>
    private void ApplyExplosiveEffect(StardewValley.Monsters.Monster monster)
    {
        int explosionDamage = (int)CurrentAmmoType!.SpecialEffectValue;
        var location = monster.currentLocation;
        if (location == null) return;

        foreach (var character in location.characters)
        {
            if (character is StardewValley.Monsters.Monster nearbyMonster &&
                Vector2.Distance(nearbyMonster.Position, monster.Position) < 128)
            {
                nearbyMonster.takeDamage(explosionDamage, 0, 0, false, 0.0, Game1.player);
            }
        }
    }

    /// <summary>
    /// 应用后坐力
    /// </summary>
    private void ApplyRecoil(Farmer shooter)
    {
        // 简单的屏幕震动效果
        Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite(
            "TileSheets\\animations",
            new Rectangle(0, 0, 1, 1),
            100f,
            1,
            0,
            Vector2.Zero,
            false,
            false
        ));
    }

    /// <summary>
    /// 更新逻辑
    /// </summary>
    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady) return;

        // 检查是否持有枪械
        var player = Game1.player;
        _isHoldingGun = player?.CurrentItem?.Name == RL11Data.ItemName;

        // 检查装填完成
        if (IsReloading)
        {
            if (Environment.TickCount - _reloadStartTime >= 1500)
            {
                FinishReload();
            }
        }

        // 自动装填
        if (_isHoldingGun && CurrentAmmo == 0 && !IsReloading)
        {
            Reload();
        }
        
        // 能量自然恢复（每tick恢复0.05点）
        if (_isHoldingGun && CurrentEnergy < MaxEnergy)
        {
            CurrentEnergy = Math.Min(MaxEnergy, CurrentEnergy + 1);
        }
    }

    /// <summary>
    /// 鼠标移动事件
    /// </summary>
    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (_isHoldingGun)
        {
            _mousePosition = e.NewPosition.AbsolutePixels;
            _showTargetHighlight = true;
        }
        else
        {
            _showTargetHighlight = false;
        }
    }

    /// <summary>
    /// 渲染世界（高亮鼠标位置）
    /// </summary>
    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!_isHoldingGun || !_showTargetHighlight) return;

        // 获取鼠标位置对应的瓦片
        Vector2 mouseTile = Game1.currentCursorTile;
        Vector2 tilePixelPos = mouseTile * 64f;

        // 绘制高亮框
        DrawTargetHighlight(e.SpriteBatch, tilePixelPos);
    }

    /// <summary>
    /// 绘制目标高亮
    /// </summary>
    private void DrawTargetHighlight(SpriteBatch spriteBatch, Vector2 position)
    {
        // 检查能量是否足够，决定颜色
        Color highlightColor = CurrentEnergy >= EnergyPerShot 
            ? new Color(255, 255, 0, 128)  // 黄色（能量充足）
            : new Color(255, 0, 0, 128);   // 红色（能量不足）

        // 绘制高亮边框
        int borderThickness = 2;
        int size = 64;

        // 上边框
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle((int)position.X, (int)position.Y, size, borderThickness),
            highlightColor
        );

        // 下边框
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle((int)position.X, (int)position.Y + size - borderThickness, size, borderThickness),
            highlightColor
        );

        // 左边框
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle((int)position.X, (int)position.Y, borderThickness, size),
            highlightColor
        );

        // 右边框
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle((int)position.X + size - borderThickness, (int)position.Y, borderThickness, size),
            highlightColor
        );

        // 绘制半透明填充
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle((int)position.X + borderThickness, (int)position.Y + borderThickness, 
                         size - borderThickness * 2, size - borderThickness * 2),
            new Color((byte)highlightColor.R, (byte)highlightColor.G, (byte)highlightColor.B, (byte)32)
        );
    }

    /// <summary>
    /// 商店菜单打开时添加枪械
    /// </summary>
    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ShopMenu shopMenu) return;

        string? shopOwner = GetShopOwner(shopMenu);
        if (shopOwner == null) return;

        if (shopOwner == "Pierre" || shopOwner == "Joja")
        {
            AddGunToShop(shopMenu);
        }
    }

    /// <summary>
    /// 获取商店所有者
    /// </summary>
    private string? GetShopOwner(ShopMenu shopMenu)
    {
        if (Game1.currentLocation?.Name == "SeedShop")
            return "Pierre";
        if (Game1.currentLocation?.Name == "JojaMart")
            return "Joja";
        return null;
    }

    /// <summary>
    /// 向商店添加枪械
    /// </summary>
    private void AddGunToShop(ShopMenu shopMenu)
    {
        var gun = new StardewValley.Object(RL11Data.ItemId, 1)
        {
            Name = RL11Data.ItemName,
            Price = RL11Data.ItemPrice
        };

        var itemPriceAndStock = shopMenu.itemPriceAndStock;
        var forSale = shopMenu.forSale;

        if (itemPriceAndStock != null && forSale != null)
        {
            itemPriceAndStock[gun] = new ItemStockInformation(
                price: RL11Data.ItemPrice,
                stock: int.MaxValue
            );
            forSale.Add(gun);
        }
    }

    /// <summary>
    /// 处理输入
    /// </summary>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady) return;
        if (!_isHoldingGun) return;

        // 射击
        if (e.Button == SButton.MouseLeft)
        {
            var player = Game1.player;
            if (player != null)
            {
                Vector2 targetPos = e.Cursor.AbsolutePixels;
                Fire(player, targetPos);
            }
        }

        // 手动装填
        if (e.Button == SButton.R)
        {
            Reload();
        }
    }

    /// <summary>
    /// 渲染HUD
    /// </summary>
    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (_isHoldingGun)
        {
            _gui.Draw(e.SpriteBatch);
        }
    }
}
