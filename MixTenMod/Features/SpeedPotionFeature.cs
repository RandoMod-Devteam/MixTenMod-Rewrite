using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MixTenMod.i18n;

namespace MixTenMod.Features;

/// <summary>
/// 速度剂功能
/// </summary>
public class SpeedPotionFeature
{
    private readonly IModHelper Helper;
    private int _currentSpeedStack;
    private int _originalSpeed;

    public SpeedPotionFeature(IModHelper helper)
    {
        Helper = helper;
        _currentSpeedStack = 0;
        _originalSpeed = -1;

        helper.Events.GameLoop.DayStarted += OnDayStarted;
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        helper.Events.Display.MenuChanged += OnMenuChanged;
        helper.Events.Input.ButtonPressed += OnButtonPressed;
    }

    /// <summary>
    /// 每天开始时重置速度
    /// </summary>
    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        _currentSpeedStack = 0;
        _originalSpeed = -1;
    }

    /// <summary>
    /// 更新玩家速度
    /// </summary>
    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        var player = Game1.player;
        if (player == null)
            return;

        // 保存原始速度
        if (_originalSpeed < 0)
        {
            _originalSpeed = player.speed;
        }

        // 计算目标速度
        int targetSpeed = _originalSpeed;
        if (_currentSpeedStack > 0)
        {
            targetSpeed += _currentSpeedStack * (int)SpeedPotionData.SpeedMultiplierPerStack;
        }

        // 应用速度
        if (player.speed != targetSpeed)
        {
            player.speed = targetSpeed;
        }
    }

    /// <summary>
    /// 商店菜单打开时添加速度剂
    /// </summary>
    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ShopMenu shopMenu)
            return;

        // 检查是否是皮埃尔商店或Joja商店
        string? shopOwner = GetShopOwner(shopMenu);
        if (shopOwner == null)
            return;

        if (shopOwner == "Pierre" || shopOwner == "Joja")
        {
            AddSpeedPotionToShop(shopMenu);
        }
    }

    /// <summary>
    /// 获取商店所有者
    /// </summary>
    private string? GetShopOwner(ShopMenu shopMenu)
    {
        // 通过商店名称判断
        if (Game1.currentLocation?.Name == "SeedShop")
            return "Pierre";
        if (Game1.currentLocation?.Name == "JojaMart")
            return "Joja";

        return null;
    }

    /// <summary>
    /// 向商店添加速度剂
    /// </summary>
    private void AddSpeedPotionToShop(ShopMenu shopMenu)
    {
        // 创建速度剂物品 - 使用特定的物品ID格式
        var speedPotion = new StardewValley.Object("787", 1)  // 使用游戏中存在的物品ID作为基础
        {
            Name = SpeedPotionData.ItemName,
            Price = SpeedPotionData.ItemPrice
        };

        // 添加到商店
        var itemPriceAndStock = shopMenu.itemPriceAndStock;
        var forSale = shopMenu.forSale;

        if (itemPriceAndStock != null && forSale != null)
        {
            itemPriceAndStock[speedPotion] = new ItemStockInformation(
                price: SpeedPotionData.ItemPrice,
                stock: int.MaxValue
            );
            forSale.Add(speedPotion);
        }
    }

    /// <summary>
    /// 处理物品使用
    /// </summary>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        if (!e.Button.IsActionButton())
            return;

        var player = Game1.player;
        if (player?.CurrentItem == null)
            return;

        // 检查是否是速度剂 - 使用ItemId或Name匹配
        if (!IsSpeedPotion(player.CurrentItem))
            return;

        // 检查是否已达到最大层数
        if (_currentSpeedStack >= SpeedPotionData.MaxStack)
        {
            Game1.showRedMessage(I18n.Tr("message.speed-potion.max"));
            return;
        }

        // 消耗物品并增加速度
        if (player.CurrentItem.Stack > 1)
        {
            player.CurrentItem.Stack--;
        }
        else
        {
            player.removeFirstOfThisItemFromInventory(player.CurrentItem.QualifiedItemId);
        }

        _currentSpeedStack++;

        // 显示效果
        ShowSpeedEffect();
    }

    /// <summary>
    /// 检查物品是否是速度剂
    /// </summary>
    private bool IsSpeedPotion(Item item)
    {
        // 通过多种方式检查是否是速度剂
        if (item.Name == SpeedPotionData.ItemName)
            return true;
        
        if (item.QualifiedItemId == "(O)" + SpeedPotionData.ItemId)
            return true;
            
        // 检查显示名称是否匹配
        string displayName = item.DisplayName ?? item.Name;
        if (displayName == SpeedPotionData.GetDisplayName())
            return true;
            
        return false;
    }

    /// <summary>
    /// 显示速度提升效果
    /// </summary>
    private void ShowSpeedEffect()
    {
        var player = Game1.player;
        if (player == null)
            return;

        // 播放音效
        Game1.playSound("gulp");

        // 显示浮动文字
        var tokens = new Dictionary<string, string> { { "multiplier", _currentSpeedStack.ToString() } };
        string message = I18n.Tr("message.speed-potion.use", tokens);
        Game1.addHUDMessage(new HUDMessage(message, HUDMessage.newQuest_type));

        // 添加粒子效果
        AddSpeedParticles(player);
    }

    /// <summary>
    /// 添加速度粒子效果
    /// </summary>
    private void AddSpeedParticles(Farmer player)
    {
        var location = player.currentLocation;
        if (location == null)
            return;

        // 创建风/速度效果的粒子
        for (int i = 0; i < 10; i++)
        {
            Vector2 position = player.Position + new Vector2(
                Game1.random.Next(-32, 32),
                Game1.random.Next(-32, 32)
            );

            location.temporarySprites.Add(new TemporaryAnimatedSprite(
                "TileSheets\\animations",
                new Rectangle(0, 128, 64, 64),
                60f,
                4,
                0,
                position,
                false,
                false
            ));
        }
    }
}
