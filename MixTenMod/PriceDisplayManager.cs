using Microsoft.Xna.Framework;
using MixTenMod.config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MixTenMod;
public class PriceDisplayManager
{
    private readonly IMonitor _monitor;
    private readonly ModConfig _config;
    
    private Item? _hoveredItem;
    private Vector2 _hoverPosition = Vector2.Zero;

    public PriceDisplayManager(IMonitor monitor,ModConfig config)
    {
        _monitor = monitor;
        _config = config;
    }

    public void AfterHudRender()
    {
        if (!_config.EnableMod || !_config.ShowPrices || 
            !Context.IsWorldReady || _hoveredItem == null ||
            Game1.activeClickableMenu == null) 
            return;
        
        try
        {
            if (_hoveredItem.canBeShipped())
            {
                int price = _hoveredItem.sellToStorePrice();
                string priceText = $"{price}金";
                Vector2 textSize = Game1.smallFont.MeasureString(priceText);
                
                // 绘制背景
                Game1.spriteBatch.Draw(
                    Game1.staminaRect,
                    new Rectangle((int)_hoverPosition.X, (int)_hoverPosition.Y - 20, (int)textSize.X + 8, (int)textSize.Y + 8),
                    new Color(0, 0, 0, 150)
                );
                
                // 绘制价格文本
                Game1.spriteBatch.DrawString(
                    Game1.smallFont,
                    priceText,
                    new Vector2(_hoverPosition.X + 4, _hoverPosition.Y - 16),
                    Color.Gold
                );
            }
        }
        catch (Exception ex)
        {
            _monitor.Log($"绘制物品价格时出错: {ex.Message}", LogLevel.Debug);
        }
    }
    
    private Item? GetHoveredItem()
    {
        try
        {
            if (Game1.activeClickableMenu is ItemGrabMenu grabMenu)
                return grabMenu.hoveredItem;
            
            return Game1.player.CurrentItem;
        }
        catch
        {
            return null;
        }
    }
    public void DrawCustomPriceTooltip(Item item, Vector2 position)
    {
        if (!item.canBeShipped()) return;
    
        try
        {
            int price = item.sellToStorePrice();
            string priceText = $"{price}金";
            Vector2 textSize = Game1.smallFont.MeasureString(priceText);
        
            // 绘制背景
            Game1.spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle((int)position.X, (int)position.Y - 20, (int)textSize.X + 8, (int)textSize.Y + 8),
                new Color(0, 0, 0, 150)
            );
        
            // 绘制价格文本
            Game1.spriteBatch.DrawString(
                Game1.smallFont,
                priceText,
                new Vector2(position.X + 4, position.Y - 16),
                Color.Gold
            );
        }
        catch (Exception ex)
        {
            _monitor.Log($"绘制自定义价格提示时出错: {ex.Message}", LogLevel.Debug);
        }
    }
    public void Update()
    {
        _hoveredItem = GetHoveredItem();
        _hoverPosition = new Vector2(Game1.getMouseX(), Game1.getMouseY());
    }

}